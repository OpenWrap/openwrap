using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;
using OpenFileSystem.IO;
using OpenWrap.Exports;

namespace OpenWrap.Repositories
{
    public class CachedZipPackage : ZipPackage
    {
        readonly IEnumerable<IExportBuilder> _builders;
        readonly IDirectory _cacheDirectoryPathPath;
        
        UncompressedPackage _cachedPackage;

        public CachedZipPackage(IPackageRepository source, IFile packageFile, IDirectory cacheDirectoryPath, IEnumerable<IExportBuilder> builders)
            : base(packageFile)
        {
            Source = source;
            _cacheDirectoryPathPath = cacheDirectoryPath;
            _builders = builders;
        }
        
        public override IPackage Load()
        {
            if (_cachedPackage == null)
            {
                ExtractPackage(PackageFile, _cacheDirectoryPathPath);

                _cachedPackage = new UncompressedPackage(Source, PackageFile, _cacheDirectoryPathPath, _builders);
            }
            return _cachedPackage;
        }
        // TODO: Replace with clean OFS-based zip methods
        static void ExtractPackage(IFile wrapFile, IDirectory destinationDirectory)
        {
            var nt = new WindowsNameTransform(destinationDirectory.Path.FullPath);
            using (var zipFile = new ZipFile(wrapFile.OpenRead()))
            {
                foreach (ZipEntry zipEntry in zipFile)
                {
                    if (zipEntry.IsFile)
                    {
                        var filePath = nt.TransformFile(zipEntry.Name);
                        using (var targetFile = destinationDirectory.FileSystem.GetFile(filePath).MustExist().OpenWrite())
                        using (var sourceFile = zipFile.GetInputStream(zipEntry))
                            sourceFile.CopyTo(targetFile);
                        // TODO: restore last write time here by adding it to OFS
                    }
                }
            }
        }
    }
}
