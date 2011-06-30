using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using OpenFileSystem.IO;
using OpenWrap.IO;
using OpenWrap.PackageManagement.Exporters;
using OpenWrap.PackageModel;
using OpenWrap.PackageModel.Serialization;
using OpenWrap.Repositories;
using OpenWrap.Runtime;
using Path = OpenFileSystem.IO.Path;

namespace OpenWrap.PackageManagement.Packages
{
    public class UncompressedPackage : IPackage
    {
        static readonly TraceSource _log = new TraceSource("openwrap", SourceLevels.All);
        readonly IFile _originalPackageFile;
        IPackageDescriptor _descriptor;

        public UncompressedPackage(IPackageRepository source,
                                   IFile originalPackageFile,
                                   IDirectory wrapCacheDirectory)
        {
            _originalPackageFile = originalPackageFile;
            BaseDirectory = wrapCacheDirectory;
            // get the descriptor file inside the package
            var descriptorName = originalPackageFile.NameWithoutExtension;
            Source = source;
            var wrapDescriptor = wrapCacheDirectory.Files("*.wrapdesc").SingleOrDefault();
            if (wrapDescriptor == null)
                throw new InvalidOperationException("Could not find descriptor in wrap cache directory, or there are multiple .wrapdesc files in the package.");
            var versionFile = wrapCacheDirectory.GetFile("version");
            _descriptor = new PackageDescriptorReaderWriter().Read(wrapDescriptor);
            PackageInfo = new DefaultPackageInfo(originalPackageFile.Name,
                                                versionFile.Exists ? versionFile.Read(x => x.ReadString().ToVersion()) : null,
                                                _descriptor);
            Identifier = new PackageIdentifier(Name, Version);
        }

        public bool Anchored
        {
            get { return PackageInfo.Anchored; }
        }

        public DateTimeOffset Created
        {
            get
            {
                if (_originalPackageFile.LastModifiedTimeUtc != null) return _originalPackageFile.LastModifiedTimeUtc.Value;
                return DateTimeOffset.UtcNow;
            }
        }

        public ICollection<PackageDependency> Dependencies
        {
            get { return PackageInfo.Dependencies; }
        }

        public string Description
        {
            get { return PackageInfo.Description; }
        }

        public string FullName
        {
            get { return Name + "-" + Version; }
        }

        public PackageIdentifier Identifier { get; private set; }

        public string Name
        {
            get { return PackageInfo.Name; }
        }

        public bool Nuked
        {
            get { return false; }
        }

        public IPackageRepository Source { get; private set; }

        public Version Version
        {
            get { return PackageInfo.Version; }
        }

        protected IDirectory BaseDirectory { get; set; }

        protected DefaultPackageInfo PackageInfo { get; set; }

        public IEnumerable<IGrouping<string, Exports.IFile>> Content
        {
            get
            {
                return (from directory in BaseDirectory.Directories()
                        from file in directory.Files("**\\*")
                        let relativePath = file.Parent.Path.MakeRelative(BaseDirectory.Path)
                        select new FileExportItem(new Path(relativePath), file, this))
                        .ToLookup(x => x.Path, x=>(Exports.IFile)x);
            }
        }

        public IPackageDescriptor Descriptor
        {
            get { return _descriptor; }
        }

        public Stream OpenStream()
        {
            return _originalPackageFile.OpenRead();
        }

        public IPackage Load()
        {
            return this;
        }
    }
}