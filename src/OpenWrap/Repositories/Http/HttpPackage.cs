using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenFileSystem.IO;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.Packages;
using OpenWrap.PackageModel;
using OpenWrap.Runtime;


namespace OpenWrap.Repositories.Http
{
    public class HttpPackage : IPackage
    {
        readonly IFileSystem _fileSystem;
        readonly IHttpRepositoryNavigator _httpNavigator;
        readonly LazyValue<PackageIdentifier> _identifier;
        readonly PackageEntry _package;
        IPackage _loadedPackage;

        public HttpPackage(IFileSystem fileSystem,
                           IPackageRepository source,
                           IHttpRepositoryNavigator httpNavigator,
                           PackageEntry package)
        {
            _fileSystem = fileSystem;
            _httpNavigator = httpNavigator;
            _package = package;
            _identifier = Lazy.Is(() => new PackageIdentifier(Name, Version));
            Source = source;
        }

        public bool Anchored
        {
            get { return false; }
        }

        public string Title
        {
            get { return _package.Title; }
        }

        public DateTimeOffset Created
        {
            get { return _package.CreationTime; }
        }

        public ICollection<PackageDependency> Dependencies { get; set; }

        public string Description
        {
            get { return _package.Description; }
        }

        public string FullName
        {
            get { return Name + "-" + Version; }
        }

        public PackageIdentifier Identifier
        {
            get { return _identifier; }
        }

        public string Name
        {
            get { return _package.Name; }
        }

        public bool Nuked
        {
            get { return _package.Nuked; }
        }

        public bool IsValid
        {
            get { return true; }
        }

        public IPackageRepository Source { get; private set; }

        public SemanticVersion Version
        {
            get { return _package.Version; }
        }

        public Stream OpenStream()
        {
            VerifyLoaded();
            return _loadedPackage.OpenStream();
        }

        public IEnumerable<IGrouping<string, Exports.IFile>> Content
        {
            get { return Enumerable.Empty<IGrouping<string, Exports.IFile>>(); }
        }

        public IPackageDescriptor Descriptor
        {
            get
            {
                VerifyLoaded();
                return _loadedPackage.Descriptor;
            }
        }
        public IPackage Load()
        {
            return this;
        }

        void VerifyLoaded()
        {
            if (_loadedPackage != null) return;

            IFile temporaryFile = _fileSystem.CreateTempFile();
            using (var sourceStream = _httpNavigator.LoadPackage(_package))
            using (var destinationStream = temporaryFile.OpenWrite())
                sourceStream.CopyTo(destinationStream);

            _loadedPackage = new CachedZipPackage(Source, temporaryFile, _fileSystem.CreateTempDirectory()).Load();
        }
    }
}