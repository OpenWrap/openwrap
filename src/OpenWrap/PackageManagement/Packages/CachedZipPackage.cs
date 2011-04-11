using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;
using OpenFileSystem.IO;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;
using StreamExtensions = OpenWrap.IO.StreamExtensions;

namespace OpenWrap.PackageManagement.Packages
{
    public class CachedZipPackage : ZipPackage
    {
        readonly IDirectory _cacheDirectoryPathPath;

        UncompressedPackage _cachedPackage;

        public CachedZipPackage(IPackageRepository source, IFile packageFile, IDirectory cacheDirectoryPath)
                : base(packageFile)
        {
            Source = source;
            _cacheDirectoryPathPath = cacheDirectoryPath;
        }

        public override IPackage Load()
        {
            if (_cachedPackage == null)
            {
                if (!ExtractPackage(PackageFile, _cacheDirectoryPathPath)) return null;

                _cachedPackage = new UncompressedPackage(Source, PackageFile, _cacheDirectoryPathPath);
            }
            return _cachedPackage;
        }
    }
}