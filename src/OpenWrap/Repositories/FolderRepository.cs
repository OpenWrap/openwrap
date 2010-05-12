using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;
using OpenRasta.Wrap.Dependencies;
using OpenRasta.Wrap.Repositories;

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
            ? (IWrapPackageInfo)new FolderWrapPackage(wrapFile, cacheDirectory, new[] { new AssemblyReferenceExportBuider() })
            : (IWrapPackageInfo)new ZippedWrapPackage(wrapFile, cacheDirectory,  new[] { new AssemblyReferenceExportBuider() }))
                .ToLookup(x => x.Name);
        }

        public string BasePath { get; set; }

        public ILookup<string, IWrapPackageInfo> PackagesByName { get; private set; }

        public IWrapPackageInfo Find(WrapDependency dependency)
        {
            return PackagesByName.Find(dependency);
        }

    }

    public class ZippedWrapPackage : IWrapPackageInfo
    {
        readonly FileInfo _wrapFile;
        readonly string _cacheDirectory;
        readonly IEnumerable<IExportBuilder> _builders;
        FolderWrapPackage _cachedPackage;

        public ZippedWrapPackage(FileInfo wrapFile, string cacheDirectory, IEnumerable<IExportBuilder> builders)
        {
            _wrapFile = wrapFile;
            _cacheDirectory = cacheDirectory;
            _builders = builders;

            LoadDescriptor();
        }

        void LoadDescriptor()
        {
            using (var zip = new ZipFile(_wrapFile.FullName))
            {
                var descriptor = zip.Cast<ZipEntry>().FirstOrDefault(x => x.Name.EndsWith(".wrapdesc"));
                if (descriptor == null)
                    throw new InvalidOperationException("The package '{0}' doesn't contain a valid .wrapdesc file.");
                using (var stream = zip.GetInputStream(descriptor))
                    Descriptor = new WrapDescriptorParser().ParseFile(descriptor.Name, stream);
            }
        }

        protected WrapDescriptor Descriptor { get; set; }

        public ICollection<WrapDependency> Dependencies { get { return Descriptor.Dependencies; } }
        public string Name
        {
            get { return Descriptor.Name; }
        }

        public Version Version
        {
            get { return Descriptor.Version; }
        }

        public IWrapPackage Load()
        {
            if (_cachedPackage == null)
            {
                new FastZip().ExtractZip(_wrapFile.FullName, _cacheDirectory, FastZip.Overwrite.Always, x => true, null, null, true);
                _cachedPackage = new FolderWrapPackage(_wrapFile, _cacheDirectory, _builders);
            }
            return _cachedPackage;
        }
    }

    public static class PackagesExtensions
    {
        public static IWrapPackageInfo Find(this ILookup<string, IWrapPackageInfo> packages, WrapDependency dependency)
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