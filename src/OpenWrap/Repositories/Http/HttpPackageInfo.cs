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
        readonly DateTimeOffset _lastModifiedTimeUtc;
        readonly bool _nuked;

        public HttpPackageInfo(IFileSystem fileSystem,
                              IPackageRepository source,
                              IHttpRepositoryNavigator httpNavigator,
                              PackageItem package)
        {
            Source = source;
            _fileSystem = fileSystem;
            _httpNavigator = httpNavigator;
            _package = package;

            _lastModifiedTimeUtc = package.CreationTime;
            _nuked = package.Nuked;
            
            Dependencies = (from dependency in _package.Dependencies
                            let strings = dependency.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)
                            where strings.Length >= 1
                            let dependencyName = strings[0]
                            where !string.IsNullOrEmpty(dependencyName)
                            select (PackageDependency)new PackageDependencyBuilder(dependencyName)
                                        .SetVersionVertices(DependsParser.ParseVersions(strings.Skip(1).ToArray()))
                            )
                            .ToList();
        }

        public PackageIdentifier Identifier
        {
            get { return new PackageIdentifier(Name, Version); }
        }

        public ICollection<PackageDependency> Dependencies { get; set; }

        public string FullName
        {
            get { return Name + "-" + Version; }
        }

        public DateTimeOffset Created { get { return _package.CreationTime; } }

        public bool Anchored
        {
            get { return false; }
        }

        public string Name { get{ return _package.Name;}}

        public IPackageRepository Source { get; private set; }
        public Version Version { get{ return _package.Version;} }
        public string Description { get { return _package.Description; } }
        public bool Nuked { get { return _nuked; } }

        public IPackage Load()
        {
            // get the file from the server
            return new HttpPackage(_fileSystem, Source, _httpNavigator, _package);
        }
    }
}