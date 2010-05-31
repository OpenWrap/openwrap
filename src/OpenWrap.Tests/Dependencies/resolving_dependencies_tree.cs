using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace OpenRasta.Wrap.Tests.Dependencies
{
    public class when_resolving_unavailable_dependencies : context.dependency_manager_context
    {
        [Test]
        public void not_found_package_fails_resolution()
        {
            given_dependency("depends rings-of-power");

            when_resolving_packages();

            Resolve.IsSuccess.ShouldBeFalse();
            Resolve.Dependencies.ShouldHaveCountOf(1);
            Resolve.Dependencies.First().Package.ShouldBeNull();
        }

        
    }
    public class when_conflicting_dependencies : context.dependency_manager_context
    {
        [Test]
        public void the_resolving_fails()
        {
            given_local_package("sauron-1.0.0");
            given_local_package("sauron-1.1.0");
            given_local_package("rings-of-power-1.0.0", "depends sauron = 1.0.0");
            given_local_package("one-ring-to-rule-them-all-1.0.0", "depends sauron = 1.1.0");
            given_local_package("tolkien-1.0.0", "depends rings-of-power", "depends one-ring-to-rule-them-all");

            given_dependency("depends tolkien");

            when_resolving_packages();

            Resolve.IsSuccess.ShouldBeFalse();
            Resolve.Dependencies.Count().ShouldBe(5);
            Resolve.Dependencies.ToLookup(x => x.Package.Name)["sauron"].Count().ShouldBe(2);
        }

        [Test]
        public void local_declaration_overrides_packages()
        {
            given_local_package("sauron-1.0.0");
            given_local_package("sauron-1.1.0");
            given_local_package("rings-of-power-1.0.0", "depends sauron = 1.0.0");
            given_local_package("one-ring-to-rule-them-all-1.0.0", "depends sauron = 1.1.0");
            given_local_package("tolkien-1.0.0", "depends rings-of-power", "depends one-ring-to-rule-them-all");

            given_dependency("depends tolkien");
            given_dependency("depends sauron = 1.0.0");

            when_resolving_packages();

            Resolve.IsSuccess.ShouldBeTrue();
            Resolve.Dependencies.Count().ShouldBe(4);
            Resolve.Dependencies.ToLookup(x => x.Package.Name)["sauron"]
                .ShouldHaveCountOf(1)
                .First().Package.Version.ShouldBe(new Version(1, 0, 0));
        }
    }
    public class when_resolving_available_dependencies : context.dependency_manager_context
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
        public void dependency_on_user_package_is_resolved()
        {
            given_user_package("rings-of-power-1.0.0");
            given_dependency("depends rings-of-power");

            when_resolving_packages();

            Resolve.IsSuccess.ShouldBeTrue();
            Resolve.Dependencies.First().Package.ShouldNotBeNull()
                .Source.ShouldBe(UserRepository);
        }

        [Test]
        public void dependency_on_remote_package_is_resolved()
        {
            given_remote1_package("rings-of-power-1.0.0");
            given_dependency("depends rings-of-power");

            when_resolving_packages();

            Resolve.IsSuccess.ShouldBeTrue();
            Resolve.Dependencies.First().Package.ShouldNotBeNull()
                .Source.ShouldBe(RemoteRepository);
        }


        [Test]
        public void local_package_found_before_user_package()
        {
            given_remote1_package("rings-of-power-1.1.0");
            given_local_package("rings-of-power-1.0.0");
            given_dependency("depends rings-of-power");

            when_resolving_packages();

            Resolve.IsSuccess.ShouldBeTrue();
            var dependency = Resolve.Dependencies.First();

            dependency.Package.ShouldNotBeNull()
                .Source.ShouldBe(LocalRepository);
            dependency.Package.Version.ShouldBe(new Version(1, 0,0));
        }
    }
    namespace context
    {
        public class dependency_manager_context : OpenWrap.Testing.context
        {
            protected WrapDescriptor DependencyDescriptor;
            protected InMemoryRepository LocalRepository;
            protected InMemoryRepository UserRepository;
            protected InMemoryRepository RemoteRepository;
            protected DependencyResolutionResult Resolve;

            protected override void SetUp()
            {
                base.SetUp();
                DependencyDescriptor = new WrapDescriptor
                {
                    Name = "test",
                    Version = new Version(1, 0)
                };
                LocalRepository = new InMemoryRepository();
                UserRepository = new InMemoryRepository();
                RemoteRepository = new InMemoryRepository();
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

            public IPackageInfo Publish(string packageFileName, Stream packageStream)
            {
                var package = new InMemoryPackage
                {
                    Name = WrapNameUtility.GetName(packageFileName),
                    Version = WrapNameUtility.GetVersion(packageFileName)
                };
                Packages.Add(package);
                return package;
            }
        }
    }
}
