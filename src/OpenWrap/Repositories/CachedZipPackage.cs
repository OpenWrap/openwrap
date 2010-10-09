using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using OpenWrap.Exports;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;
using OpenFileSystem.IO;

namespace OpenWrap.Repositories
{
    public class CachedZipPackage : IPackageInfo
    {
        readonly IEnumerable<IExportBuilder> _builders;
        readonly IDirectory _cacheDirectoryPathPath;
        readonly IFile _wrapFile;
        UncompressedPackage _cachedPackage;

        public CachedZipPackage(IPackageRepository source, IFile wrapFile, IDirectory cacheDirectoryPath, IEnumerable<IExportBuilder> builders)
        {
            Source = source;
            _wrapFile = wrapFile;
            _cacheDirectoryPathPath = cacheDirectoryPath;
            _builders = builders;
            
            LoadDescriptor();
        }

        public ICollection<PackageDependency> Dependencies
        {
            get { return Descriptor.Dependencies; }
        }

        public string Name
        {
            get { return Descriptor.Name; }
        }

        public Version Version
        {
            get { return Descriptor.Version; }
        }

        protected WrapDescriptor Descriptor { get; set; }

        public IPackage Load()
        {
            if (_cachedPackage == null)
            {
                ExtractWrapFile(_wrapFile, _cacheDirectoryPathPath);

                _cachedPackage = new UncompressedPackage(Source, _wrapFile, _cacheDirectoryPathPath, _builders);
            }
            return _cachedPackage;
        }
        // TODO: Replace with clean OFS-based zip methods
        void ExtractWrapFile(IFile wrapFile, IDirectory destinationDirectory)
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

        public string FullName
        {
            get { return Name + "-" + Version; }
        }

        public DateTime? LastModifiedTimeUtc
        {
            get { return _wrapFile.LastModifiedTimeUtc; }
        }

        public IPackageRepository Source
        {
            get; set;
        }

        // TODO: once zip OpenFileSystem support has been implemented, replace the WrapDescriptorParser to be able to parse the directory name, version and version header in the correct precedence order, for use in other repositories.
        void LoadDescriptor()
        {
            using(var zipStream = _wrapFile.OpenRead())
            using (var zip = new ZipFile(zipStream))
            {
                var entries = zip.Cast<ZipEntry>();
                var descriptor = entries.FirstOrDefault(x => x.Name.EndsWith(".wrapdesc"));
                if (descriptor == null)
                    throw new InvalidOperationException(string.Format("The package '{0}' doesn't contain a valid .wrapdesc file.", _wrapFile.Name));
                using (var stream = zip.GetInputStream(descriptor))
                    Descriptor = new WrapDescriptorParser().ParseFile(new ZipWrapperFile(zip, descriptor), stream);
                if (Descriptor.Version == null)
                {
                    var versionFile = entries.SingleOrDefault(x => string.Compare(x.Name, "version", StringComparison.OrdinalIgnoreCase) == 0);
                    if (versionFile == null)
                    {
                        Descriptor.Version = PackageNameUtility.GetVersion(this._wrapFile.NameWithoutExtension);
                    }
                    else
                    {
                        using (var versionStream = zip.GetInputStream(versionFile))
                            Descriptor.Version = new Version(versionStream.ReadString(Encoding.UTF8));
                    }
                }
                if (Descriptor.Version == null)
                    throw new InvalidOperationException("The package '{0}' doesn't have a valid version, looked in the 'wrapdesc' file, in 'version' and in the package file-name.");
            }
        }
    }
}