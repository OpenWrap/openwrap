using System;
using System.Collections.Generic;
using System.IO;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;
using OpenWrap.Exports;

namespace OpenWrap.Repositories
{
    public class XmlPackage : IPackage
    {
        readonly IHttpNavigator _httpNavigator;
        Uri _link;
        IPackage _loadedPackage;
        IEnumerable<IExportBuilder> _builders;

        public XmlPackage(IPackageRepository source, IHttpNavigator httpNavigator, Uri link, string name, Version version, IEnumerable<IExportBuilder> builders)
        {
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
            string tempFileName = Path.GetTempFileName();

            using (var stream = _httpNavigator.LoadFile(_link))
            using (var fileStream = File.OpenWrite(tempFileName))
                stream.CopyTo(fileStream);

            _loadedPackage = new ZipPackage(Source, new FileInfo(tempFileName), Path.GetTempPath(), _builders).Load();
        }

    }
}