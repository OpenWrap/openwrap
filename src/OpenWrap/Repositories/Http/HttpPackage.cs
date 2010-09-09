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

        public HttpPackage(IFileSystem fileSystem,
            IPackageRepository source, 
            IHttpRepositoryNavigator httpNavigator, 
            PackageItem package)
        {
            _fileSystem = fileSystem;
            _httpNavigator = httpNavigator;
            _package = package;
            Source = source;
        }

        public ICollection<WrapDependency> Dependencies { get; set; }
        public string Name { get { return _package.Name; } }
        public Version Version { get { return _package.Version; } }
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

        public DateTime? LastModifiedTimeUtc
        {
            get{ return _package.LastModifiedTimeUtc; }
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
            using (var stream = _httpNavigator.LoadPackage(_package))
            {
                stream.CopyTo(temporaryFile.OpenWrite());
                // we don't dispose here, the file will get disposed and deleted on exit if we're lucky.
            }

            _loadedPackage = new CachedZipPackage(Source, temporaryFile, _fileSystem.CreateTempDirectory(), Enumerable.Empty<IExportBuilder>(), false).Load();
        }

    }
}