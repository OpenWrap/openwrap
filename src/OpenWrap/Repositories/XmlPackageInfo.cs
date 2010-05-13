using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenRasta.IO;
using OpenRasta.Wrap.Dependencies;
using OpenRasta.Wrap.Repositories;
using OpenRasta.Wrap.Resources;
using OpenRasta.Wrap.Sources;

namespace OpenWrap.Repositories
{
    public class XmlPackageInfo : IPackageInfo
    {
        readonly IHttpNavigator _httpNavigator;
        Uri _link;
        IEnumerable<IExportBuilder> _builders;


        public XmlPackageInfo(IHttpNavigator httpNavigator, string name, string version, Uri link, IEnumerable<string> depends, IEnumerable<IExportBuilder> builders)
        {
            Name = name;
            Version = new Version(version);
            _httpNavigator = httpNavigator;
            _builders = builders;
            _link = link;
            Dependencies = (from dependency in depends
                            let strings = dependency.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)
                            where strings.Length >= 1
                            let dependencyName = strings[0]
                            where !string.IsNullOrEmpty(dependencyName)
                            select new WrapDependency
                            {
                                Name = dependencyName,
                                VersionVertices = WrapDependencyParser.Parse(strings.Skip(1).ToArray()).ToList()
                            }).ToList();
        }

        public ICollection<WrapDependency> Dependencies { get; set; }
        public string Name { get; set; }
        public Version Version { get; set; }

        public IPackage Load()
        {
            // get the file from the server
            return new XmlPackage(_httpNavigator, _link, Name, Version, _builders);
        }
    }

    public class XmlPackage : IPackage
    {
        readonly IHttpNavigator _httpNavigator;
        Uri _link;
        IPackage _loadedPackage;
        IEnumerable<IExportBuilder> _builders;

        public XmlPackage(IHttpNavigator httpNavigator, Uri link, string name, Version version, IEnumerable<IExportBuilder> builders)
        {
            _httpNavigator = httpNavigator;
            _builders = builders;
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

        public IWrapExport GetExport(string exportName, WrapRuntimeEnvironment environment)
        {
            VerifyLoaded();
            return _loadedPackage.GetExport(exportName, environment);
        }

        void VerifyLoaded()
        {
            if (_loadedPackage != null) return;
            string tempFileName = Path.GetTempFileName();

            using (var stream = _httpNavigator.LoadFile(_link))
            using (var fileStream = File.OpenWrite(tempFileName))
                stream.CopyTo(fileStream);

            _loadedPackage = new ZipPackage(new FileInfo(tempFileName), Path.GetTempPath(), _builders).Load();
        }

        public void Persist(string folder)
        {
            VerifyLoaded();
            _loadedPackage.Persist(folder);
        }
    }
}