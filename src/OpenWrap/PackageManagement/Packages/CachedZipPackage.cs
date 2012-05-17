using System;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;
using OpenFileSystem.IO;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;


namespace OpenWrap.PackageManagement.Packages
{
    public class CachedZipPackage : ZipFilePackage
    {
        readonly IDirectory _cacheDirectoryPathPath;

        UncompressedPackage _cachedPackage;

        public CachedZipPackage(IPackageRepository source, IFile packageFile, IDirectory cacheDirectoryPath)
                : base(packageFile)
        {
            Check.NotNull(source, "source");
            Check.NotNull(packageFile, "packageFile");
            Check.NotNull(cacheDirectoryPath, "cacheDirectoryPath");
            if (packageFile.Exists == false) throw new ArgumentException(string.Format("File '{0}' does not exist.", packageFile.Path), "packageFile");

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