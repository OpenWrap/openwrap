using System.IO;
using System.Linq;
using OpenRasta.Wrap.Dependencies;
using OpenRasta.Wrap.Repositories;
using OpenWrap.Repositories;

namespace OpenRasta.Wrap.Sources
{
    /// <summary>
    /// Provides a repository that can read packages in a folder using the default structure.
    /// </summary>
    public class FolderRepository : IWrapRepository
    {
        /// <exception cref="DirectoryNotFoundException"><c>DirectoryNotFoundException</c>.</exception>
        public FolderRepository(string basePath)
        {

            BasePath = basePath;
            if (!Directory.Exists(basePath))
                throw new DirectoryNotFoundException(string.Format("The directory '{0}' doesn't exist.", basePath));

            var baseDirectory = new DirectoryInfo(basePath);

            PackagesByName = (from wrapFile in baseDirectory.GetFiles("*.wrap")
                              let wrapFileName = Path.GetFileNameWithoutExtension(wrapFile.Name)
                              let cacheDirectory = FileSystem.CombinePaths(baseDirectory.FullName, "cache", wrapFileName)
                              let packageVersion = WrapNameUtility.GetVersion(wrapFileName)
                              where packageVersion != null
                              select Directory.Exists(cacheDirectory)
            ? (IPackageInfo)new FolderWrapPackage(wrapFile, cacheDirectory, new[] { new AssemblyReferenceExportBuider() })
            : (IPackageInfo)new ZipPackage(wrapFile, cacheDirectory,  new[] { new AssemblyReferenceExportBuider() }))
                .ToLookup(x => x.Name);
        }

        public string BasePath { get; set; }

        public ILookup<string, IPackageInfo> PackagesByName { get; private set; }

        public IPackageInfo Find(WrapDependency dependency)
        {
            return PackagesByName.Find(dependency);
        }

    }

    public static class PackagesExtensions
    {
        public static IPackageInfo Find(this ILookup<string, IPackageInfo> packages, WrapDependency dependency)
        {
            if (!packages.Contains(dependency.Name))
                return null;
            return (from package in packages[dependency.Name]
                    where package.Version != null && dependency.IsFulfilledBy(package.Version)
                    orderby package.Version descending
                    select package).FirstOrDefault();
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