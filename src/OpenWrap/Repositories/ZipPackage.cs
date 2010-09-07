using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        readonly bool _anchorsEnabled;
        readonly IDirectory _cacheDirectoryPathPath;
        readonly IFile _wrapFile;
        UncompressedPackage _cachedPackage;

        public ZipPackage(IPackageRepository source, IFile wrapFile, IDirectory cacheDirectoryPath, IEnumerable<IExportBuilder> builders, bool anchorsEnabled)
        {
            Source = source;
            _wrapFile = wrapFile;
            _cacheDirectoryPathPath = cacheDirectoryPath;
            _builders = builders;
            _anchorsEnabled = anchorsEnabled;

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
                new FastZip().ExtractZip(_wrapFile.Path.FullPath, _cacheDirectoryPathPath.MustExist().Path.FullPath, FastZip.Overwrite.Always, x => true, null, null, true);
                _cachedPackage = new UncompressedPackage(Source, _wrapFile, _cacheDirectoryPathPath, _builders, _anchorsEnabled);
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
                        Descriptor.Version = WrapNameUtility.GetVersion(this._wrapFile.NameWithoutExtension);
                    }
                    else
                    {
                        using (var versionStream = zip.GetInputStream(versionFile))
                            Descriptor.Version = new Version(versionStream.ReadString(Encoding.UTF8));
                    }
                }
                if (Descriptor.Version == null)
                    throw new InvalidOperationException("The pacakge '{0}' doesn't have a valid version, looked in the 'wrapdesc' file, in 'version' and in the package file-name.");
            }
        }
    }
}