using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenWrap.Dependencies;
using OpenFileSystem.IO;
using OpenWrap.Exports;

namespace OpenWrap.Repositories.Http
{
    public class HttpPackage : IPackage
    {
        readonly IFileSystem _fileSystem;
        readonly IHttpRepositoryNavigator _httpNavigator;
        readonly PackageItem _package;
        IPackage _loadedPackage;
        LazyValue<PackageIdentifier> _identifier;

        public HttpPackage(IFileSystem fileSystem,
            IPackageRepository source, 
            IHttpRepositoryNavigator httpNavigator, 
            PackageItem package)
        {
            _fileSystem = fileSystem;
            _httpNavigator = httpNavigator;
            _package = package;
            _identifier = Lazy.Is(()=>new PackageIdentifier(Name, Version));
            Source = source;
        }

        public PackageIdentifier Identifier
        {
            get { return _identifier; }
        }

        public ICollection<PackageDependency> Dependencies { get; set; }
        public string Name { get { return _package.Name; } }
        public Version Version { get { return _package.Version; } }
        public string Description { get { return _package.Description; } }
        public bool Nuked { get { return _package.Nuked; } }

        public IPackage Load()
        {
            return this;
        }

        public IPackageRepository Source
        {
            get; private set;
        }

        public IExport GetExport(string exportName, ExecutionEnvironment environment)
        {
            return null;
        }

        public string FullName
        {
            get { return Name + "-" + Version; }
        }

        public DateTimeOffset Created
        {
            get{ return _package.CreationTime; }
        }

        public bool Anchored
        {
            get { return false; }
        }

        public Stream OpenStream()
        {
            VerifyLoaded();
            return _loadedPackage.OpenStream();
        }

        void VerifyLoaded()
        {
            if (_loadedPackage != null) return;

            IFile temporaryFile = _fileSystem.CreateTempFile();
            using (var sourceStream = _httpNavigator.LoadPackage(_package))
            using (var destinationStream = temporaryFile.OpenWrite())
                sourceStream.CopyTo(destinationStream);

            _loadedPackage = new CachedZipPackage(Source, temporaryFile, _fileSystem.CreateTempDirectory(), Enumerable.Empty<IExportBuilder>()).Load();
        }

    }
}