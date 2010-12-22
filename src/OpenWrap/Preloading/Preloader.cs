using System;
using System.Collections.Generic;
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
        public static IEnumerable<string> GetPackageFolders(RemoteInstall remote, string systemRepositoryPath, params string[] packageNamesToLoad)
        {
            var regex = new Regex(string.Format(@"^(?<name>{0})-(?<version>\d+(\.\d+(\.\d+(\.\d+)?)?)?)$", string.Join("|", packageNamesToLoad.ToArray())), RegexOptions.IgnoreCase);
            EnsurePackagesUnzippedInProjectRepository();

            var bootstrapAssemblies = GetLatestPackagesForProjectRepository(regex);
            return bootstrapAssemblies.Count() == 0
                           ? ((bootstrapAssemblies = GetLatestPackagesForSystemRepository(systemRepositoryPath, regex)).Count() == 0 && remote.Enabled
                                      ? TryDownloadPackages(systemRepositoryPath, packageNamesToLoad, regex, remote)
                                      : bootstrapAssemblies)
                           : bootstrapAssemblies;
        }

        public static IEnumerable<KeyValuePair<Assembly, string>> LoadAssemblies(IEnumerable<string> packageFolders)
        {
            return (
                           from asm in packageFolders
                           from assemblyPath in CombinePaths(asm, "bin-net35", "bin-net40")
                           where Directory.Exists(assemblyPath)
                           from file in Directory.GetFiles(assemblyPath, "*.dll")
                           let assembly = TryLoadAssembly(file)
                           where assembly != null
                           select new KeyValuePair<Assembly, string>(assembly, file)
                   ).ToList();
        }

        static IEnumerable<string> CombinePaths(string packageFolder, params string[] subFolders)
        {
            return subFolders.Select(x => Path.Combine(packageFolder, x));
        }

        static void EnsurePackagesUnzippedInProjectRepository()
        {
            foreach (var extraction in from directory in GetSelfAndParents(Environment.CurrentDirectory)
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
                    ZipArchive.Extract(stream, extraction.cacheFolderForWrap.FullName);
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

        static IEnumerable<string> GetLatestPackageDirectories(Regex regex, IEnumerable<DirectoryInfo> cacheDirectories)
        {
            foreach (var dir in cacheDirectories)
            {
                var all= (
                               from uncompressedFolder in dir.GetDirectories()
                               let match = regex.Match(uncompressedFolder.Name)
                               where match.Success
                               let version = new Version(match.Groups["version"].Value)
                               let name = match.Groups["name"].Value
                               group new { name, folder = uncompressedFolder, version } by name
                               into tuplesByName
                               select tuplesByName.OrderByDescending(x => x.version).First().folder.FullName
                         )
                         .ToList();

                if (all.Count > 0)
                    return all;
            }
            return Enumerable.Empty<string>();
        }

        static IEnumerable<string> GetLatestPackagesForProjectRepository(Regex regex)
        {
            var projectRepositories = (from directory in GetSelfAndParents(Environment.CurrentDirectory)
                                     where directory.Exists
                                     let cacheDirectory = GetCacheDirectoryFromProjectDirectory(directory)
                                     where cacheDirectory != null && cacheDirectory.Exists
                                     select cacheDirectory).ToList();
            return projectRepositories.Count == 0
                           ? Enumerable.Empty<string>()
                           : GetLatestPackageDirectories(regex, projectRepositories);
        }

        static IEnumerable<string> GetLatestPackagesForSystemRepository(string systemRepositoryPath, Regex regex)
        {
            return GetLatestPackageDirectories(regex, new List<DirectoryInfo> { GetCacheDirectoryFromRepositoryDirectory(new DirectoryInfo(systemRepositoryPath)) });
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

        static IEnumerable<string> TryDownloadPackages(string systemRepositoryPath, IEnumerable<string> packageNames, Regex regex, RemoteInstall remote)
        {
            var systemRepositoryCacheDirectory = GetCacheDirectoryFromRepositoryDirectory(new DirectoryInfo(systemRepositoryPath));
            if (!systemRepositoryCacheDirectory.Exists)
                systemRepositoryCacheDirectory.Create();


            var indexUri = new Uri(new Uri(remote.ServerUri, UriKind.Absolute), "index.wraplist");

            var content = remote.Client.DownloadData(indexUri);
            XDocument document = XDocument.Load(new XmlTextReader(new MemoryStream(content)));

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
                                           packageNames.Contains(nameAttribute.Value, StringComparer.OrdinalIgnoreCase)
                                     group new { Name = nameAttribute.Value, Version = versionAttribute.Value, Href = packageSource } by nameAttribute.Value
                                     into byNameGroup
                                     select byNameGroup.OrderByDescending(x => x.Version)
                                             .Select(x => new { x.Name, Href = new Uri(x.Href, UriKind.RelativeOrAbsolute) })
                                             .FirstOrDefault();

            foreach (var packageName in packageNames)
            {
                var foundPackage = packagesToDownload.FirstOrDefault(x => x.Name.Equals(packageName, StringComparison.OrdinalIgnoreCase));
                if (foundPackage == null)
                    throw new FileNotFoundException(string.Format("Package '{0}' not found in repository.", packageName));
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
                    ZipArchive.Extract(wrapFileStream, extractFolder);
            }
            return GetLatestPackagesForSystemRepository(systemRepositoryPath, regex);
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
            RemoteInstall(bool enabled, string uri, INotifyDownload notifier)
            {
                Enabled = enabled;
                ServerUri = uri;
                Client = new NotifyProgressWebClient(notifier);
            }

            public static RemoteInstall None
            {
                get { return new RemoteInstall(false, null, null); }
            }

            public NotifyProgressWebClient Client { get; set; }
            public bool Enabled { get; set; }
            public string ServerUri { get; set; }

            public static RemoteInstall FromServer(string uri, INotifyDownload notifier)
            {
                return new RemoteInstall(true, uri, notifier);
            }
        }
    }
}