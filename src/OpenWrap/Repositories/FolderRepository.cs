using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenWrap.Dependencies;
using OpenWrap.Exports;

namespace OpenWrap.Repositories
{
    /// <summary>
    /// Provides a repository that can read packages in a folder using the default structure.
    /// </summary>
    public class FolderRepository : IPackageRepository
    {
        /// <exception cref="DirectoryNotFoundException"><c>DirectoryNotFoundException</c>.</exception>
        public FolderRepository(string packageBasePath)
        {
            BasePath = packageBasePath;
            if (!Directory.Exists(packageBasePath))
                throw new DirectoryNotFoundException(string.Format("The directory '{0}' doesn't exist.", packageBasePath));

            var baseDirectory = new DirectoryInfo(packageBasePath);

            Packages = (from wrapFile in baseDirectory.GetFiles("*.wrap")
                        let wrapFileName = Path.GetFileNameWithoutExtension(wrapFile.Name)
                        let cacheDirectory = GetCacheDirectory(wrapFileName)
                        let packageVersion = WrapNameUtility.GetVersion(wrapFileName)
                        where packageVersion != null
                        select Directory.Exists(cacheDirectory)
                                   ? (IPackageInfo)new UncompressedPackage(this, wrapFile, cacheDirectory, ExportBuilders.All)
                                   : (IPackageInfo)new ZipPackage(this, wrapFile, cacheDirectory, ExportBuilders.All)).ToList();
        }

        public string BasePath { get; set; }

        public ILookup<string, IPackageInfo> PackagesByName
        {
            get { return Packages.ToLookup(x => x.Name); }
        }

        protected List<IPackageInfo> Packages { get; set; }

        public IPackageInfo Find(WrapDependency dependency)
        {
            return PackagesByName.Find(dependency);
        }

        public IPackageInfo Publish(string packageFileName, Stream packageStream)
        {
            var filePath = Path.Combine(BasePath, packageFileName);

            if (File.Exists(filePath))
                return null;
            using (var file = File.OpenWrite(filePath))
                packageStream.CopyTo(file);

            var newPackage = new ZipPackage(this, new FileInfo(filePath), GetCacheDirectory(filePath), ExportBuilders.All);
            Packages.Add(newPackage);
            return newPackage;
        }

        string GetCacheDirectory(string wrapFileName)
        {
            return FileSystem.CombinePaths(BasePath, "cache", wrapFileName);
        }
    }
    public static class ExportBuilders
    {
        static readonly List<IExportBuilder> _exportBuilders = new List<IExportBuilder>
        {
            new AssemblyReferenceExportBuilder()
        };
        public static ICollection<IExportBuilder> All { get { return _exportBuilders; } }
    }
}