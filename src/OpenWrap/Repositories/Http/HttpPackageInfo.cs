using System;
using System.Collections.Generic;
using System.Linq;
using OpenFileSystem.IO;
using OpenWrap.PackageModel;
using OpenWrap.PackageModel.Parsers;

namespace OpenWrap.Repositories.Http
{
    public class HttpPackageInfo : IPackageInfo
    {
        readonly IFileSystem _fileSystem;
        readonly IHttpRepositoryNavigator _httpNavigator;
        readonly DateTimeOffset _lastModifiedTimeUtc;
        readonly bool _nuked;
        readonly PackageEntry _package;

        public HttpPackageInfo(IFileSystem fileSystem,
                               IPackageRepository source,
                               IHttpRepositoryNavigator httpNavigator,
                               PackageEntry package)
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

        public bool Anchored
        {
            get { return false; }
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
            get { return new PackageIdentifier(Name, Version); }
        }

        public string Name
        {
            get { return _package.Name; }
        }

        public bool Nuked
        {
            get { return _nuked; }
        }

        public IPackageRepository Source { get; private set; }

        public Version Version
        {
            get { return _package.Version; }
        }

        public IPackage Load()
        {
            // get the file from the server
            return new HttpPackage(_fileSystem, Source, _httpNavigator, _package);
        }
    }
}