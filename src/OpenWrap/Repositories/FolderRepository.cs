using System.IO;
using System.Linq;
using OpenWrap.Exports;
using OpenWrap.Repositories;
using OpenWrap.Dependencies;

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

            PackagesByName = (from wrapFile in baseDirectory.GetFiles("*.wrap")
                              let wrapFileName = Path.GetFileNameWithoutExtension(wrapFile.Name)
                              let cacheDirectory = FileSystem.CombinePaths(baseDirectory.FullName, "cache", wrapFileName)
                              let packageVersion = WrapNameUtility.GetVersion(wrapFileName)
                              where packageVersion != null
                              select Directory.Exists(cacheDirectory)
            ? (IPackageInfo)new UncompressedPackage(this, wrapFile, cacheDirectory, new[] { new AssemblyReferenceExportBuilder() })
            : (IPackageInfo)new ZipPackage(this, wrapFile, cacheDirectory,  new[] { new AssemblyReferenceExportBuilder() }))
                .ToLookup(x => x.Name);
        }

        public string BasePath { get; set; }

        public ILookup<string, IPackageInfo> PackagesByName { get; private set; }

        public IPackageInfo Find(WrapDependency dependency)
        {
            return PackagesByName.Find(dependency);
        }

    }

    public static class FileSystem
    {
        public static string CombinePaths(params string[] paths)
        {
            if (paths == null || paths.Length < 1) return null;
            
            var path = paths[0];
            foreach (var pathToCombine in paths.Skip(1))
                path = Path.Combine(path, pathToCombine);
            return path;
        }
    }
}