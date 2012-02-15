using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap;
using OpenWrap.PackageManagement.DependencyResolvers;
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
        StrategyResolver _resolver;

        const string PACKAGE_NAME="test";

        public nuked_package_resolution()
        {
            _resolver = new StrategyResolver();
        }
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
            
            ResolvedVersion = _resolver.TryResolveDependencies(
                new PackageDescriptor() { Dependencies = {_packageDependency}},
                new[]{_repo}
            ).SuccessfulPackages.First().Identifier.Version.ToString();
        }
    }
}