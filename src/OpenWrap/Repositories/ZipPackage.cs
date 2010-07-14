using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;
using OpenWrap.Exports;
using OpenWrap.Dependencies;
using OpenFileSystem.IO;
using OpenWrap.Repositories;

namespace OpenWrap.Repositories
{
    public class ZipPackage : IPackageInfo
    {
        readonly IEnumerable<IExportBuilder> _builders;
        readonly IDirectory _cacheDirectoryPath;
        readonly IFile _wrapFile;
        UncompressedPackage _cachedPackage;

        public ZipPackage(IPackageRepository source, IFile wrapFile, IDirectory wrapCacheDirectory, IEnumerable<IExportBuilder> builders)
        {
            Source = source;
            _wrapFile = wrapFile;
            _cacheDirectoryPath = wrapCacheDirectory;
            _builders = builders;

            LoadDescriptor();
        }

        public ICollection<WrapDependency> Dependencies
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
                new FastZip().ExtractZip(_wrapFile.Path.FullPath, _cacheDirectoryPath.EnsureExists().Path.FullPath, FastZip.Overwrite.Always, x => true, null, null, true);
                _cachedPackage = new UncompressedPackage(Source, _wrapFile, _cacheDirectoryPath, _builders);
            }
            return _cachedPackage;
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

        void LoadDescriptor()
        {
            using(var zipStream = _wrapFile.OpenRead())
            using (var zip = new ZipFile(zipStream))
            {
                var descriptor = zip.Cast<ZipEntry>().FirstOrDefault(x => x.Name.EndsWith(".wrapdesc"));
                if (descriptor == null)
                    throw new InvalidOperationException(string.Format("The package '{0}' doesn't contain a valid .wrapdesc file.", _wrapFile.Name));
                using (var stream = zip.GetInputStream(descriptor))
                    Descriptor = new WrapDescriptorParser().ParseFile(new ZipWrapperFile(zip, descriptor), stream);
            }
        }
    }
}