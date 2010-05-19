using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace OpenRasta.Wrap.Tests.Dependencies
{
    public class resolving_dependencies_tree : context.dependency_manager_context
    {
        [Test]
        public void single_package_is_resolved()
        {
            given_local_package("rings-of-power-1.0.0");
            given_dependency("depends rings-of-power");

            when_resolving_packages();

            Resolve.IsSuccess.ShouldBeTrue();
            Resolve.Dependencies.ShouldHaveCountOf(1);
        }

        [Test]
        public void not_found_package_fails_resolution()
        {
            given_dependency("depends rings-of-power");

            when_resolving_packages();

            Resolve.IsSuccess.ShouldBeFalse();
            Resolve.Dependencies.ShouldHaveCountOf(1);
            Resolve.Dependencies.First().Package.ShouldBeNull();
        }

        [Test]
        public void dependency_on_user_package_is_resolved()
        {
            given_user_package("rings-of-power-1.0.0");
            given_dependency("depends rings-of-power");

            when_resolving_packages();

            Resolve.IsSuccess.ShouldBeTrue();
            Resolve.Dependencies.First().Package.ShouldNotBeNull()
                .Source.ShouldBe(UserRepository);
        }
    }
    namespace context
    {
        public class dependency_manager_context : OpenWrap.Testing.context
        {
            protected WrapDescriptor DependencyDescriptor;
            protected InMemoryRepository LocalRepository = new InMemoryRepository();
            protected InMemoryRepository UserRepository = new InMemoryRepository();
            protected InMemoryRepository RemoteRepository = new InMemoryRepository();
            protected DependencyResolutionResult Resolve;

            protected override void SetUp()
            {
                base.SetUp();
                DependencyDescriptor = new WrapDescriptor
                {
                    Name = "test",
                    Version = new Version(1, 0)
                };
            }
            protected void given_dependency(string dependency)
            {
                new WrapDependencyParser().Parse(dependency, DependencyDescriptor);
            }
            protected void given_local_package(string name, params string[] dependencies)
            {
                Add(LocalRepository, name, dependencies);

            }
            protected void given_user_package(string name, params string[] dependencies)
            {
                Add(UserRepository, name, dependencies);

            }
            protected void given_remote1_package(string name, params string[] dependencies)
            {
                Add(RemoteRepository, name, dependencies);

            }

            void Add(InMemoryRepository repository, string name, string[] dependencies)
            {
                var package = new InMemoryPackage
                {
                    Name = WrapNameUtility.GetName(name),
                    Version = WrapNameUtility.GetVersion(name),
                    Source = repository,
                    Dependencies = dependencies.Select(x =>
                        WrapDependencyParser.ParseDependency(x)).ToList()
                };
                repository.Packages.Add(package);
            }
            protected void when_resolving_packages()
            {
                Resolve = new PackageManager().TryResolveDependencies(DependencyDescriptor,
                                                                      LocalRepository,
                                                                      UserRepository,
                                                                      new[] { RemoteRepository });
            }
        }
        public class InMemoryPackage : IPackageInfo
        {
            public ICollection<WrapDependency> Dependencies { get; set; }
            public string Name { get; set; }
            public Version Version { get; set; }
            public IPackage Load()
            {
                throw new NotImplementedException();
            }

            public IPackageRepository Source { get; set; }
        }

        public class InMemoryRepository : IPackageRepository
        {
            public List<IPackageInfo> Packages = new List<IPackageInfo>();

            public ILookup<string, IPackageInfo> PackagesByName
            {
                get { return Packages.ToLookup(x => x.Name); }
            }

            public IPackageInfo Find(WrapDependency dependency)
            {
                return PackagesByName.Find(dependency);
            }
        }
    }
}
