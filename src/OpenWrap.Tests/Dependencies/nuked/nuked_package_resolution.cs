using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap;
using OpenWrap.PackageManagement.Packages;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;

namespace Tests.Dependencies.contexts
{
    public class nuked_package_resolution : OpenWrap.Testing.context
    {
        protected string ResolvedVersion;
        private PackageDependencyBuilder _packageDependency
            = new PackageDependencyBuilder(PACKAGE_NAME);
        private readonly List<IPackageInfo> _versions = new List<IPackageInfo>();
        InMemoryRepository _repo = new InMemoryRepository("in-mem");

        const string PACKAGE_NAME="test";

        protected void given_package_version(string version)
        {
            _repo.Packages.Add(new InMemoryPackage
            {
                Name = PACKAGE_NAME,
                SemanticVersion = SemanticVersion.TryParseExact(version)
            });
        }

        protected void given_nuked_package_version(string version)
        {
            _repo.Packages.Add(new InMemoryPackage
            {
                Name = PACKAGE_NAME,
                SemanticVersion = SemanticVersion.TryParseExact(version),
                Nuked = true
            });
        }

        protected void given_dependency(VersionVertex vertex)
        {
            _packageDependency.VersionVertex(vertex);
        }

        protected void when_resolving()
        {
            ResolvedVersion = _repo
                .PackagesByName.FindAll(_packageDependency).First()
                .SemanticVersion.ToString();
        }
    }
}