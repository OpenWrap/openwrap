using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;
using OpenRasta.Wrap.Dependencies;
using OpenRasta.Wrap.Repositories;
using OpenRasta.Wrap.Sources;

namespace OpenWrap.Repositories
{
    public class ZipPackage : IPackageInfo
    {
        readonly IEnumerable<IExportBuilder> _builders;
        readonly string _cacheDirectory;
        readonly FileInfo _wrapFile;
        UncompressedPackage _cachedPackage;

        public ZipPackage(FileInfo wrapFile, string wrapCacheDirectory, IEnumerable<IExportBuilder> builders)
        {
            _wrapFile = wrapFile;
            _cacheDirectory = wrapCacheDirectory;
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
                new FastZip().ExtractZip(_wrapFile.FullName, _cacheDirectory, FastZip.Overwrite.Always, x => true, null, null, true);
                _cachedPackage = new UncompressedPackage(_wrapFile, _cacheDirectory, _builders);
            }
            return _cachedPackage;
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
    }
}