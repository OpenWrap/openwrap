using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Dependencies;
using OpenFileSystem.IO;

namespace OpenWrap.Repositories.Http
{
    public class HttpPackageInfo : IPackageInfo
    {
        readonly IFileSystem _fileSystem;
        readonly IHttpRepositoryNavigator _httpNavigator;
        readonly PackageItem _package;
        readonly DateTime? _lastModifiedTimeUtc;
        


        public HttpPackageInfo(IFileSystem fileSystem,
                              IPackageRepository source,
                              IHttpRepositoryNavigator httpNavigator,
                              PackageItem package)
        {
            Source = source;
            _fileSystem = fileSystem;
            _httpNavigator = httpNavigator;
            _package = package;

            _lastModifiedTimeUtc = package.LastModifiedTimeUtc;
            
            Dependencies = (from dependency in _package.Dependencies
                            let strings = dependency.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)
                            where strings.Length >= 1
                            let dependencyName = strings[0]
                            where !string.IsNullOrEmpty(dependencyName)
                            select new WrapDependency
                            {
                                Name = dependencyName,
                                VersionVertices = DependsParser.ParseVersions(strings.Skip(1).ToArray()).ToList()
                            }).ToList();
        }

        public ICollection<WrapDependency> Dependencies { get; set; }

        public string FullName
        {
            get { return Name + "-" + Version; }
        }

        public DateTime? LastModifiedTimeUtc { get { return _package.LastModifiedTimeUtc; } }
        public string Name { get{ return _package.Name;}}

        public IPackageRepository Source { get; private set; }
        public Version Version { get{ return _package.Version;} }

        public IPackage Load()
        {
            // get the file from the server
            return new HttpPackage(_fileSystem, Source, _httpNavigator, _package);
        }
    }
}