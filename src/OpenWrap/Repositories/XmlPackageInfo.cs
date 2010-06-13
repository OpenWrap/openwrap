using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Exports;
using OpenWrap.Dependencies;
using OpenWrap.IO;
using OpenWrap.Repositories;

namespace OpenWrap.Repositories
{
    public class XmlPackageInfo : IPackageInfo
    {
        readonly IFileSystem _fileSystem;
        readonly IHttpNavigator _httpNavigator;
        Uri _link;
        IEnumerable<IExportBuilder> _builders;


        public XmlPackageInfo(IFileSystem fileSystem, IPackageRepository source, IHttpNavigator httpNavigator, string name, string version, Uri link, IEnumerable<string> depends, IEnumerable<IExportBuilder> builders)
        {
            Name = name;
            Version = new Version(version);
            Source = source;
            _fileSystem = fileSystem;
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
                                VersionVertices = WrapDependencyParser.ParseVersions(strings.Skip(1).ToArray()).ToList()
                            }).ToList();
        }

        public ICollection<WrapDependency> Dependencies { get; set; }
        public string Name { get; set; }
        public Version Version { get; set; }

        public string FullName
        {
            get { return Name + "-" + Version; }
        }
        public IPackage Load()
        {
            // get the file from the server
            return new XmlPackage(_fileSystem, Source, _httpNavigator, _link, Name, Version, _builders);
        }

        public IPackageRepository Source { get; set; }
    }
}