using System;
using System.Collections.Generic;
using System.IO;
using OpenWrap.Dependencies;
using OpenWrap.IO;
using OpenWrap.Repositories;
using OpenWrap.Exports;

namespace OpenWrap.Repositories
{
    public class XmlPackage : IPackage
    {
        readonly IFileSystem _fileSystem;
        readonly IHttpNavigator _httpNavigator;
        Uri _link;
        IPackage _loadedPackage;
        IEnumerable<IExportBuilder> _builders;

        public XmlPackage(IFileSystem fileSystem, IPackageRepository source, IHttpNavigator httpNavigator, Uri link, string name, Version version, IEnumerable<IExportBuilder> builders)
        {
            _fileSystem = fileSystem;
            _httpNavigator = httpNavigator;
            _builders = builders;
            Source = source;
            Name = name;
            Version = version;
            _link = link;
        }

        public ICollection<WrapDependency> Dependencies { get; set; }
        public string Name { get; set; }
        public Version Version { get; set; }
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
            VerifyLoaded();
            return _loadedPackage.GetExport(exportName, environment);
        }

        public string FullName
        {
            get { return Name + "-" + Version; }
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
            using (var stream = _httpNavigator.LoadFile(_link))
            {
                
                stream.CopyTo(temporaryFile.OpenWrite());
                // we don't dispose here, the file will get disposed and deleted on exit if we're lucky.
            }

            _loadedPackage = new ZipPackage(Source, temporaryFile, _fileSystem.GetTempDirectory(), _builders).Load();
        }

    }
}