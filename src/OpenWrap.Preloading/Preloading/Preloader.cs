using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using OpenWrap.PackageManagement;
using OpenWrap.PackageModel;
using OpenWrap.PackageModel.Parsers;
using OpenWrap.Preloading.TinySharpZip;

namespace OpenWrap.Preloading
{
    public static class Preloader
    {
        static Preloader()
        {
            Extractor = ZipArchive.Extract;
        }

        public static Action<Stream, string> Extractor { get; set; }

        public static IEnumerable<string> GetPackageFolders(RemoteInstall remote, string projectPath, string systemPath, params string[] packageNamesToLoad)
        {
            DirectoryInfo systemDirectory = systemPath == null ? null : new DirectoryInfo(systemPath);

            var bootstrapPackagePaths = Enumerable.Empty<string>();
            var fileNotFound = new List<PackageMissingException>();


            if (projectPath != null)
            {
                try
                {
                    bootstrapPackagePaths = GetLatestPackagesForProjectDirectory(packageNamesToLoad, projectPath);
                    if (bootstrapPackagePaths.Count() >= packageNamesToLoad.Length) return bootstrapPackagePaths;
                }
                catch (PackageMissingException e)
                {
                    fileNotFound.Add(e);
                }
            }

            if (systemDirectory != null)
            {
                
                try
                {
                    bootstrapPackagePaths = GetLatestPackagesForSystemRepository(GetRepositoryDirectoryFromProjectDirectory(systemDirectory), packageNamesToLoad);
                }
                catch (PackageMissingException e)
                {
                    fileNotFound.Add(e);
                }
            }

            if ((fileNotFound.Any() || bootstrapPackagePaths.Count() < packageNamesToLoad.Length) && systemDirectory != null && remote.Enabled)
                return TryDownloadPackagesToRepository(GetRepositoryDirectoryFromProjectDirectory(systemDirectory), packageNamesToLoad, remote);

            if (bootstrapPackagePaths.Count() >= packageNamesToLoad.Length)
                return bootstrapPackagePaths;

            throw new PackageMissingException(fileNotFound.SelectMany(_ => _.Paths).ToList());
        }

        public static IEnumerable<KeyValuePair<Assembly, string>> LoadAssemblies(IEnumerable<string> packageFolders)
        {
            return (
                           from asm in packageFolders
                           from assemblyPath in CombinePaths(asm, GetFxVersions().ToArray())
                           where Directory.Exists(assemblyPath)
                           from file in Directory.GetFiles(assemblyPath, "*.dll").Concat(Directory.GetFiles(assemblyPath, "*.exe"))
                           let assembly = TryLoadAssembly(file)
                           where assembly != null
                           select new KeyValuePair<Assembly, string>(assembly, file)
                   ).ToList();
        }

        public static IEnumerable<KeyValuePair<AssemblyName, string>> LocateAssemblies(IEnumerable<string> packageFolders)
        {
            return (from asm in packageFolders
                    from assemblyPath in CombinePaths(asm, GetFxVersions().ToArray())
                    where Directory.Exists(assemblyPath)
                    from file in Directory.GetFiles(assemblyPath, "*.dll").Concat(Directory.GetFiles(assemblyPath, "*.exe"))
                    let assemblyName = AssemblyName.GetAssemblyName(file)
                    select new KeyValuePair<AssemblyName, string>(assemblyName, file))
                    .ToList();
        }

        static void AddPackageAndDependents(XDocument document, string currentPackage, List<string> packageNames)
        {
            if (!packageNames.Contains(currentPackage, StringComparer.OrdinalIgnoreCase)) packageNames.Add(currentPackage);
            foreach (var dependent in GetDependents(document, currentPackage))
            {
                if (packageNames.Contains(dependent, StringComparer.OrdinalIgnoreCase) == false)
                {
                    packageNames.Add(dependent);
                    AddPackageAndDependents(document, dependent, packageNames);
                }
            }
        }

        static IEnumerable<string> CombinePaths(string packageFolder, params string[] subFolders)
        {
            return subFolders.Select(x => Path.Combine(packageFolder, x));
        }

        static void EnsurePackagesUnzippedInRepository(DirectoryInfo repositoryPath)
        {
            var cacheDirectory = GetCacheDirectoryFromRepositoryDirectory(repositoryPath);
            foreach (var extraction in from wrapFile in repositoryPath.GetFiles("*.wrap")
                                       let wrapFileName = Path.GetFileNameWithoutExtension(wrapFile.Name)
                                       where !string.IsNullOrEmpty(wrapFileName)
                                       let cacheFolderForWrap = new DirectoryInfo(Path.Combine(cacheDirectory.FullName, wrapFileName))
                                       where cacheFolderForWrap.Exists == false
                                       select new { wrapFile, cacheFolderForWrap })
            {
                extraction.cacheFolderForWrap.Create();
                extraction.cacheFolderForWrap.Refresh();
                using (var stream = extraction.wrapFile.OpenRead())
                    Extractor(stream, extraction.cacheFolderForWrap.FullName);
            }
            repositoryPath.Refresh();
        }

        static DirectoryInfo EnsureSubFolderExists(DirectoryInfo wrapDirectoryInfo, string subfolder)
        {
            var di = new DirectoryInfo(Path.Combine(wrapDirectoryInfo.FullName, subfolder));
            if (!di.Exists)
                di.Create();
            di.Refresh();
            return di;
        }

        static string EnsureTrailingDirSeparator(string fullName)
        {
            return fullName[fullName.Length - 1] == Path.DirectorySeparatorChar || fullName[fullName.Length - 1] == Path.AltDirectorySeparatorChar
                           ? fullName
                           : fullName + Path.DirectorySeparatorChar;
        }

        static DirectoryInfo GetCacheDirectoryFromRepositoryDirectory(DirectoryInfo directory)
        {
            try
            {
                if (directory == null) return null;
                var cacheDirectoryFromRepositoryDirectory =
                        EnsureSubFolderExists(directory, "_cache");
                return cacheDirectoryFromRepositoryDirectory;
            }
            catch (IOException)
            {
                return null;
            }
        }

        static IEnumerable<string> GetDependents(XDocument document, string packageName)
        {
            return from packageElement in document.Descendants("wrap")
                   let nameAttrib = packageElement.Attribute("name")
                   where nameAttrib != null && nameAttrib.Value.Equals(packageName, StringComparison.OrdinalIgnoreCase)
                   from dep in packageElement.Descendants("depends")
                   let depName = dep.Value.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()
                   where depName != null
                   select depName;
        }

        static IEnumerable<string> GetFxVersions()
        {
            if (Environment.Version.Major >= 4)
            {
                yield return "bin-net45";
                yield return "bin-net40";
            }

            yield return "bin-net35";
            yield return "bin-net30";
            yield return "bin-net20";
        }

        static IEnumerable<string> GetLatestPackagesForProjectDirectory(IEnumerable<string> packageNames, string currentDirectory)
        {
            var rootProjectDirectory = GetProjectRootDirectory(currentDirectory);
            if (rootProjectDirectory == null || rootProjectDirectory.Exists == false)
                return Enumerable.Empty<string>();
            var repoDirectory = GetRepositoryDirectoryFromProjectDirectory(rootProjectDirectory);
            var cacheDirectory = GetCacheDirectoryFromRepositoryDirectory(repoDirectory);
            EnsurePackagesUnzippedInRepository(repoDirectory);

            return ResolvePackages(cacheDirectory, packageNames);
        }

        static IEnumerable<string> GetLatestPackagesForSystemRepository(DirectoryInfo systemRepository, IEnumerable<string> packageNames)
        {
            //systemRepository = GetRepositoryDirectoryFromProjectDirectory(systemRepository);

            EnsurePackagesUnzippedInRepository(systemRepository);
            return ResolvePackages(GetCacheDirectoryFromRepositoryDirectory(systemRepository), packageNames);
        }

        static string GetPath(DirectoryInfo packageDirectory)
        {
            return EnsureTrailingDirSeparator(packageDirectory.FullName);
        }

        static DirectoryInfo GetProjectRootDirectory(string currentDirectory)
        {
            return GetSelfAndParents(currentDirectory).Where(x => x.Exists).FirstOrDefault(x => x.GetFiles("*.wrapdesc").Any());
        }

        static DirectoryInfo GetRepositoryDirectoryFromProjectDirectory(DirectoryInfo directory)
        {
            return EnsureSubFolderExists(directory, "wraps");
        }

        static IEnumerable<DirectoryInfo> GetSelfAndParents(string directoryPath)
        {
            var directory = new DirectoryInfo(directoryPath);
            do
            {
                yield return directory;
                directory = directory.Parent;
            } while (directory != null);
        }

        static Func<PreloaderPackage, bool?> ParseDepends(string line)
        {
            var lineBits = line.Split(new[] { " ", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            var name = lineBits.First();
            var versions = DependsVersionParser.ParseVersions(lineBits.Skip(1).ToArray())
                    .DefaultIfEmpty(new AnyVersionVertex());
            return package => package.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
                                      ? versions.All(_ => _.IsCompatibleWith(package.Version))
                                      : (bool?)null;
        }

        static IEnumerable<Func<PreloaderPackage, bool?>> PreloaderDependencyReader(PreloaderPackage arg)
        {
            return arg.Dependencies.Select(ParseDepends).ToList();
        }

        static IEnumerable<PreloaderPackage> PreloaderLatestVersionStrategy(IEnumerable<PreloaderPackage> arg)
        {
            return arg.OrderByDescending(_ => _.Version);
        }

        static IEnumerable<PreloaderPackage> ReadPackages(DirectoryInfo cacheDirectory)
        {
            return from packageDirectory in cacheDirectory.GetDirectories()
                   let packageFile = cacheDirectory.Parent.GetFiles(packageDirectory.Name + ".wrap").FirstOrDefault()
                   where packageFile != null
                   let wrapDescriptor = packageDirectory.GetFiles("*.wrapdesc").OrderBy(x => x.Name.Length).FirstOrDefault()
                   where wrapDescriptor != null
                   let content = File.ReadAllText(wrapDescriptor.FullName)
                   let nameMatch = Regex.Match(content, @"name\s*:\s*(?<name>\S+)", RegexOptions.Multiline | RegexOptions.IgnoreCase)
                   let semanticVersionMatch = Regex.Match(content, @"semantic-version\s*:\s*(?<version>\S+)", RegexOptions.Multiline | RegexOptions.IgnoreCase)
                   let versionMatch = Regex.Match(content, @"version\s*:\s*(?<version>[\d\.]+)", RegexOptions.Multiline | RegexOptions.IgnoreCase)
                   where nameMatch.Success
                   let version = semanticVersionMatch.Success
                                         ? SemanticVersion.TryParseExact(semanticVersionMatch.Groups["version"].Value)
                                         : (versionMatch.Success
                                                    ? SemanticVersion.TryParseExact(versionMatch.Groups["version"].Value)
                                                    : null)
                   where version != null
                   select new PreloaderPackage
                   {
                           Name = nameMatch.Groups["name"].Value,
                           Version = version,
                           Path = GetPath(packageDirectory),
                           Dependencies = (from match in Regex.Matches(content, @"depends\s*:\s*(?<dependency>.+)$", RegexOptions.Multiline).OfType<Match>()
                                           where match.Success
                                           select match.Groups["dependency"].Value).ToList()
                   };
        }

        static List<string> ResolvePackageNames(XDocument document, IEnumerable<string> packageNames)
        {
            var allPackages = new List<string>();
            foreach (var p in packageNames) AddPackageAndDependents(document, p, allPackages);
            return allPackages;
        }

        static List<string> ResolvePackages(DirectoryInfo cacheDirectory, IEnumerable<string> packageNames)
        {
            var foundPackages = ReadPackages(cacheDirectory).ToList();
            var resolver = new PackageResolverVisitor<PreloaderPackage>(
                    foundPackages,
                    PreloaderDependencyReader,
                    PreloaderLatestVersionStrategy);
            if (!resolver.Visit(packageNames
                                        .Select<string, Func<PreloaderPackage, bool?>>(
                                                name => package => package.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
                                                                           ? true
                                                                           : (bool?)null)
                                        .ToList()))
                throw new PackageMissingException(new[] { GetPath(cacheDirectory) });

            return resolver.SuccessfulPackages.Select(_ => _.Path).ToList();
        }

        static IEnumerable<string> TryDownloadPackagesToRepository(DirectoryInfo systemRepository, IEnumerable<string> packageNames, RemoteInstall remote)
        {
            var systemRepositoryCacheDirectory = GetCacheDirectoryFromRepositoryDirectory(systemRepository);

            XDocument document;
            try
            {
                var indexUri = new Uri(new Uri(remote.ServerUri, UriKind.Absolute), "index.wraplist");

                var content = remote.Client.DownloadData(indexUri);
                document = XDocument.Load(new XmlTextReader(new MemoryStream(content)));

                var packageNamesToDownload = ResolvePackageNames(document, packageNames);
                var packagesToDownload = from packageElement in document.Descendants("wrap")
                                         let nameAttribute = packageElement.Attribute("name")
                                         let semanticVersionAttribute = packageElement.Attribute("semantic-version")
                                                                        ?? packageElement.Attribute("version")
                                         let nukedAttribute = packageElement.Attribute("nuked")
                                         let packageSource = (from link in packageElement.Descendants("link")
                                                              let rel = link.Attribute("rel")
                                                              let href = link.Attribute("href")
                                                              where rel != null && rel.Value.Equals("package", StringComparison.OrdinalIgnoreCase)
                                                              select href.Value).FirstOrDefault()
                                         where nameAttribute != null &&
                                               semanticVersionAttribute != null &&
                                               packageSource != null &&
                                               (nukedAttribute == null || !nukedAttribute.Value.Equals("true", StringComparison.OrdinalIgnoreCase)) &&
                                               packageNamesToDownload.Contains(nameAttribute.Value, StringComparer.OrdinalIgnoreCase)
                                         group new { Name = nameAttribute.Value, Version = semanticVersionAttribute.Value, Href = packageSource } by nameAttribute.Value.ToLowerInvariant()
                                         into byNameGroup
                                         select byNameGroup.OrderByDescending(x => x.Version)
                                                 .Select(x => new { x.Name, Href = new Uri(x.Href, UriKind.RelativeOrAbsolute) })
                                                 .FirstOrDefault();

                foreach (var foundPackage in packagesToDownload)
                {
                    var fullPackageUri = foundPackage.Href;
                    if (!fullPackageUri.IsAbsoluteUri)
                        fullPackageUri = new Uri(new Uri(remote.ServerUri, UriKind.Absolute), fullPackageUri);

                    var fileName = fullPackageUri.Segments.Last();
                    
                    string wrapFilePath = Path.Combine(systemRepository.FullName, fileName);


                    if (File.Exists(wrapFilePath))
                        File.Delete(wrapFilePath);

                    remote.Client.DownloadFile(fullPackageUri, wrapFilePath);

                    var extractFolder = Path.Combine(systemRepositoryCacheDirectory.FullName, Path.GetFileNameWithoutExtension(fileName));

                    if (Directory.Exists(extractFolder))
                        Directory.Delete(extractFolder, true);

                    Directory.CreateDirectory(extractFolder);

                    using (var wrapFileStream = File.OpenRead(wrapFilePath))
                        Extractor(wrapFileStream, extractFolder);
                }
                systemRepository.Refresh();
                return GetLatestPackagesForSystemRepository(systemRepository, packageNames);
            }
            catch
            {
                return new string[0];
            }
        }

        static SemanticVersion TryGetVersion(string value)
        {
            return SemanticVersion.TryParseExact(value);
        }

        static Assembly TryLoadAssembly(string asm)
        {
            try
            {
                return Assembly.LoadFrom(asm);
            }
            catch
            {
                return null;
            }
        }

        public class RemoteInstall
        {
            RemoteInstall(bool enabled, string uri, INotifyDownload notifier, string proxyAddress, string proxyUsername, string proxyPassword)
            {
                Enabled = enabled;
                ServerUri = uri;
                Client = new NotifyProgressWebClient(notifier, proxyAddress, proxyUsername, proxyPassword);
            }

            public static RemoteInstall None
            {
                get { return new RemoteInstall(false, null, null, null, null, null); }
            }

            public NotifyProgressWebClient Client { get; set; }
            public bool Enabled { get; set; }
            public string ServerUri { get; set; }

            public static RemoteInstall FromServer(string uri, INotifyDownload notifier, string proxyAddress, string proxyUsername, string proxyPassword)
            {
                return new RemoteInstall(true, uri, notifier, proxyAddress, proxyUsername, proxyPassword);
            }
        }

        class PreloaderPackage
        {
            public IEnumerable<string> Dependencies;
            public string Name;
            public string Path;
            public SemanticVersion Version;
        }
    }
}
