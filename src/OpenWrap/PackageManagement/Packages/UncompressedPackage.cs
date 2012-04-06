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
        SemanticVersion _semver;

        public UncompressedPackage(IPackageRepository source,
                                   IFile originalPackageFile,
                                   IDirectory wrapCacheDirectory)
        {
            Check.NotNull(source, "source");
            if (originalPackageFile == null || originalPackageFile.Exists == false)
            {
                IsValid = false;
                return;
            }
            _originalPackageFile = originalPackageFile;
            BaseDirectory = wrapCacheDirectory;
            // get the descriptor file inside the package
            
            Source = source;
            var wrapDescriptor = wrapCacheDirectory.Files("*.wrapdesc").SingleOrDefault();

            if (wrapDescriptor == null)
            {
                IsValid = false;
                return;
            }
             
            var versionFile = wrapCacheDirectory.GetFile("version");
            _descriptor = new PackageDescriptorReaderWriter().Read(wrapDescriptor);
            _semver = _descriptor.SemanticVersion ?? _descriptor.Version.ToSemVer();
            if (_semver == null)
                _semver = versionFile.Exists ? versionFile.ReadString().ToSemVer() : null;

            IsValid = string.IsNullOrEmpty(Name) == false && _semver != null;
            Identifier = new PackageIdentifier(Name, _semver);
        }

        [Obsolete("Plase use SemanticVersion")]
        public Version Version
        {
            get { return SemanticVersion != null ? SemanticVersion.ToVersion() : _descriptor.Version; }
        }
        public bool Anchored
        {
            get { return _descriptor.Anchored; }
        }

        public string Title
        {
            get { return _descriptor.Title; }
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
            get { return _descriptor.Dependencies; }
        }

        public string Description
        {
            get { return _descriptor.Description; }
        }

        public string FullName
        {
            get { return Name + "-" + SemanticVersion; }
        }

        public PackageIdentifier Identifier { get; private set; }

        public string Name
        {
            get { return _descriptor.Name; }
        }

        public bool Nuked
        {
            get { return false; }
        }

        public bool IsValid { get; private set; }

        public IPackageRepository Source { get; private set; }

        public SemanticVersion SemanticVersion
        {
            get { return _semver; }
        }

        protected IDirectory BaseDirectory { get; set; }

        public IEnumerable<IGrouping<string, Exports.IFile>> Content
        {
            get
            {
                return (from directory in BaseDirectory.Directories()
                        from file in directory.Files("**\\*")
                        let relativePath = file.Parent.Path.MakeRelative(BaseDirectory.Path)
                        select new FileExportItem(new Path(relativePath), file, this))
                    .ToLookup(x => x.Path, x => (Exports.IFile)x);
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