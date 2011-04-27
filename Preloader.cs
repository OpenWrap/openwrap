using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
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
        public static IEnumerable<string> GetPackageFolders(RemoteInstall remote, string currentDirectory, string systemRepositoryPath, params string[] packageNamesToLoad)
        {
            var regex = new Regex(string.Format(@"^(?<name>{0})-(?<version>\d+(\.\d+(\.\d+(\.\d+)?)?)?)$", string.Join("|", packageNamesToLoad.ToArray())), RegexOptions.IgnoreCase);
            EnsurePackagesUnzippedInRepository(Environment.CurrentDirectory);

            var bootstrapPackagePaths = currentDirectory != null
                ? GetLatestPackagesForProjectRepository(packageNamesToLoad, currentDirectory) 
                : new string[0];
            if (bootstrapPackagePaths.Count() >= packageNamesToLoad.Length) return bootstrapPackagePaths;

            FileNotFoundException fileNotFound = null;
            try
            {
                bootstrapPackagePaths = GetLatestPackagesForSystemRepository(systemRepositoryPath, packageNamesToLoad);
            } 
            catch(FileNotFoundException e)
            {
                fileNotFound = e;
            }
            if ((fileNotFound != null || bootstrapPackagePaths.Count() < packageNamesToLoad.Length) && remote.Enabled)
                return TryDownloadPackages(systemRepositoryPath, packageNamesToLoad, remote);
            if (bootstrapPackagePaths.Count() >= packageNamesToLoad.Length)
                return bootstrapPackagePaths;
            throw new ArgumentException("No package present.", fileNotFound);
        }

        public static IEnumerable<KeyValuePair<Assembly, string>> LoadAssemblies(IEnumerable<string> packageFolders)
        {
            return (
                           from asm in packageFolders
                           from assemblyPath in CombinePaths(asm, "bin-net35", "bin-net40")
                           where Directory.Exists(assemblyPath)
                           from file in Directory.GetFiles(assemblyPath, "*.dll").Concat(Directory.GetFiles(assemblyPath, "*.exe"))
                           let assembly = TryLoadAssembly(file)
                           where assembly != null
                           select new KeyValuePair<Assembly, string>(assembly, file)
                   ).ToList();
        }

        static IEnumerable<string> CombinePaths(string packageFolder, params string[] subFolders)
        {
            return subFolders.Select(x => Path.Combine(packageFolder, x));
        }

        static void EnsurePackagesUnzippedInRepository(string repositoryPath)
        {
            foreach (var extraction in from directory in GetSelfAndParents(repositoryPath)
                                       where directory.Exists
                                       let wrapDirectoryInfo = new DirectoryInfo(Path.Combine(directory.FullName, "wraps"))
                                       where wrapDirectoryInfo.Exists
                                       let cacheDirectory = EnsureSubFolderExists(wrapDirectoryInfo, "_cache")
                                       from wrapFile in wrapDirectoryInfo.GetFiles("*.wrap")
                                       let wrapName = Path.GetFileNameWithoutExtension(wrapFile.Name)
                                       where !string.IsNullOrEmpty(wrapName)
                                       let cacheFolderForWrap = new DirectoryInfo(Path.Combine(cacheDirectory.FullName, wrapName))
                                       where cacheFolderForWrap.Exists == false
                                       select new { wrapFile, cacheFolderForWrap })
            {
                extraction.cacheFolderForWrap.Create();
                using (var stream = extraction.wrapFile.OpenRead())
                    Extractor(stream, extraction.cacheFolderForWrap.FullName);
            }
        }

        static DirectoryInfo EnsureSubFolderExists(DirectoryInfo wrapDirectoryInfo, string subfolder)
        {
            var di = new DirectoryInfo(Path.Combine(wrapDirectoryInfo.FullName, subfolder));
            if (!di.Exists)
                di.Create();
            return di;
        }

        static DirectoryInfo GetCacheDirectoryFromProjectDirectory(DirectoryInfo directory)
        {
            try
            {
                if (directory == null) return null;
                return GetCacheDirectoryFromRepositoryDirectory(new DirectoryInfo(Path.Combine(directory.FullName, "wraps")));
            }
            catch (IOException)
            {
                return null;
            }
        }

        static DirectoryInfo GetCacheDirectoryFromRepositoryDirectory(DirectoryInfo directory)
        {
            try
            {
                if (directory == null) return null;
                return new DirectoryInfo(Path.Combine(directory.FullName, "_cache"));
            }
            catch (IOException)
            {
                return null;
            }
        }
        /// <returns>null if packages cannot be retrieved (missing or otherwise</returns>
        static IEnumerable<string> GetLatestPackageDirectories(IEnumerable<string> packageNames, IEnumerable<DirectoryInfo> cacheDirectories)
        {
            FileNotFoundException lastFileNotFound = null;
            foreach (var cacheDirectory in cacheDirectories.Where(x=>x.Exists))
            {
                var finalList = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                try
                {
                    TryAddPackages(cacheDirectory, packageNames, finalList);
                    return finalList.Values;
                }
                catch(FileNotFoundException e)
                {
                    lastFileNotFound = e;
                    continue;
                }
            }
            if (lastFileNotFound != null)
                throw lastFileNotFound;
            return Enumerable.Empty<string>();
        }

        static void TryAddPackages(DirectoryInfo cacheDirectory, IEnumerable<string> packageNames, Dictionary<string, string> finalList)
        {
            var foundPackages = ReadPackages(cacheDirectory, packageNames).ToLookup(x => x.Name, StringComparer.OrdinalIgnoreCase);
            foreach (var package in packageNames.Where(_ => finalList.ContainsKey(_) == false))
            {
                if (foundPackages.Contains(package) == false)
                    throw new FileNotFoundException(string.Format("Package '{0}' not found in '{1}'.", package, cacheDirectory));
                TryAddPackage(cacheDirectory, foundPackages[package].OrderByDescending(x => x.Version).First(), finalList);
            }
        }
        
        static void TryAddPackage(DirectoryInfo cacheDirectory, foundPackage package, Dictionary<string, string> finalList)
        {
            if (finalList.ContainsKey(package.Name)) return;

            finalList.Add(package.Name, package.Path);
            var dependenciesToAdd = package.Dependencies.Where(_ => finalList.ContainsKey(_) == false).ToList();
            if (dependenciesToAdd.Count > 0)
                TryAddPackages(cacheDirectory, dependenciesToAdd, finalList);
        }

        static IEnumerable<foundPackage> ReadPackages(DirectoryInfo cacheDirectory, IEnumerable<string> packageNames)
        {
            if (cacheDirectory.Exists == false)
                throw new InvalidOperationException("Cache directory does not exist");
            return (
                           from package in packageNames
                           from packageDirectory in cacheDirectory.GetDirectories(package + "-*")
                           where packageDirectory.Exists
                           let packageFile = cacheDirectory.Parent.GetFiles(packageDirectory.Name + ".wrap").FirstOrDefault()
                           where packageFile != null
                           let wrapDescriptor = packageDirectory.GetFiles("*.wrapdesc").OrderBy(x => x.Name.Length).FirstOrDefault()
                           where wrapDescriptor != null
                           let content = File.ReadAllText(wrapDescriptor.FullName)
                           let nameMatch = Regex.Match(content, @"name\s*:\s*(?<name>\S+)", RegexOptions.Multiline)
                           let versionMatch = Regex.Match(content, @"version\s*:\s*(?<version>[\d\.]+)", RegexOptions.Multiline)
                           where nameMatch.Success
                           let version = versionMatch.Success ? TryGetVersion(versionMatch.Groups["version"].Value) : new Version(0,0)
                           select new foundPackage
                           { 
                               Name = nameMatch.Groups["name"].Value,
                               Version = version,
                               Descriptor = content,
                               Path = packageDirectory.FullName,
                               Dependencies = (from match in Regex.Matches(content, @"depends\s*:\s*(?<dependency>\S+)", RegexOptions.Multiline).OfType<Match>()
                                               where match.Success
                                               let value = match.Groups["dependency"].Value.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()
                                               where value != null
                                               select value).ToList()
                           });
        }

        static Version TryGetVersion(string value)
        {
            try
            {
                return new Version(value);
            }
            catch
            {
                return null;
            }
        }

        class foundPackage
        {
            public string Name;
            public Version Version;
            public string Descriptor;
            public string Path;
            public IEnumerable<string> Dependencies;
        }
        static IEnumerable<string> GetLatestPackagesForProjectRepository(IEnumerable<string> packageNames, string currentDirectory)
        {
            var projectRepositories = (from directory in GetSelfAndParents(currentDirectory)
                                       where directory.Exists
                                       let cacheDirectory = GetCacheDirectoryFromProjectDirectory(directory)
                                       where cacheDirectory != null && cacheDirectory.Exists
                                       select cacheDirectory).ToList();
            return projectRepositories.Count == 0
                           ? Enumerable.Empty<string>()
                           : GetLatestPackageDirectories(packageNames, projectRepositories);
        }

        static IEnumerable<string> GetLatestPackagesForSystemRepository(string systemRepositoryPath, IEnumerable<string> packageNames)
        {
            EnsurePackagesUnzippedInRepository(systemRepositoryPath);
            return GetLatestPackageDirectories(packageNames, new List<DirectoryInfo> { GetCacheDirectoryFromRepositoryDirectory(new DirectoryInfo(systemRepositoryPath)) });
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

        static IEnumerable<string> TryDownloadPackages(string systemRepositoryPath, IEnumerable<string> packageNames, RemoteInstall remote)
        {
            var systemRepositoryCacheDirectory = GetCacheDirectoryFromRepositoryDirectory(new DirectoryInfo(systemRepositoryPath));
            if (!systemRepositoryCacheDirectory.Exists)
                systemRepositoryCacheDirectory.Create();

            XDocument document;
            try
            {
                var indexUri = new Uri(new Uri(remote.ServerUri, UriKind.Absolute), "index.wraplist");

                var content = remote.Client.DownloadData(indexUri);
                document = XDocument.Load(new XmlTextReader(new MemoryStream(content)));

                var packageNamesToDownload = ResolvePackageNames(document, packageNames);
                var packagesToDownload = from packageElement in document.Descendants("wrap")
                                         let nameAttribute = packageElement.Attribute("name")
                                         let versionAttribute = packageElement.Attribute("version")
                                         let packageSource = (from link in packageElement.Descendants("link")
                                                              let rel = link.Attribute("rel")
                                                              let href = link.Attribute("href")
                                                              where rel != null && rel.Value.Equals("package", StringComparison.OrdinalIgnoreCase)
                                                              select href.Value).FirstOrDefault()
                                         where nameAttribute != null &&
                                               versionAttribute != null &&
                                               packageSource != null &&
                                               packageNamesToDownload.Contains(nameAttribute.Value, StringComparer.OrdinalIgnoreCase)
                                         group new { Name = nameAttribute.Value, Version = versionAttribute.Value, Href = packageSource } by nameAttribute.Value
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
                    string wrapFilePath = Path.Combine(systemRepositoryPath, fileName);


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
                return GetLatestPackagesForSystemRepository(systemRepositoryPath, packageNames);

            }
            catch
            {
                return new string[0];
            }
        }

        static List<string> ResolvePackageNames(XDocument document, IEnumerable<string> packageNames)
        {
            List<string> allPackages = new List<string>();
            foreach (var p in packageNames) AddPackageAndDependents(document, p, allPackages);
            return allPackages;
        }
        static void AddPackageAndDependents(XDocument document, string currentPackage, List<string> packageNames)
        {
            if (!packageNames.Contains(currentPackage)) packageNames.Add(currentPackage);
            foreach (var dependent in GetDependents(document, currentPackage))
            {
                if (packageNames.Contains(dependent) == false)
                {
                    packageNames.Add(dependent);
                    AddPackageAndDependents(document, dependent, packageNames);
                }
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
    }
}