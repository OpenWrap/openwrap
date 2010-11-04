using System;
using System.Linq;
using NUnit.Framework;
using OpenRasta.Wrap.Tests.Dependencies.context;
using OpenWrap;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace OpenRasta.Wrap.Tests.Dependencies
{
    public class resolving_sub_builds_from_current_directory : dependency_manager_context
    {
        public resolving_sub_builds_from_current_directory()
        {
            given_project_package("rings-of-power-1.0.0");
            given_current_directory_package("rings-of-power-1.0.0.1");
            given_dependency("depends: rings-of-power");

            when_resolving_packages();
        }

        [Test]
        public void package_found_from_current_directory()
        {
            Resolve.ResolvedPackages.Where(x => x.Package.Name == "rings-of-power")
                    .ShouldHaveCountOf(1)
                    .First().Package.Source.ShouldBe(CurrentDirectoryRepository);
        }
    }

    public class dependency_leveling_across_dependencies : dependency_manager_context
    {
        public dependency_leveling_across_dependencies()
        {
            given_project_package("rings-of-power-1.0.0");
            given_project_package("rings-of-power-2.0.0");
            given_project_package("sauron-1.0.0", "depends: rings-of-power < 3.0");
            given_project_package("frodo-1.0.0", "depends: rings-of-power = 1.0");

            given_dependency("depends: sauron");
            given_dependency("depends: frodo");

            when_resolving_packages();
        }
        [Test]
        public void common_compatible_version_is_resolved()
        {
            Resolve.ResolvedPackages.Where(x => x.PackageName == "rings-of-power")
                    .ShouldHaveCountOf(1)
                    .First().Package.Version.ShouldBe("1.0.0".ToVersion());
        }
    }

    public class when_resolving_unavailable_dependencies : dependency_manager_context
    {
        public when_resolving_unavailable_dependencies()
        {
            given_dependency("depends: rings-of-power");

            when_resolving_packages();
        }

        [Test]
        public void dependency_has_no_package()
        {
            Resolve.ResolvedPackages.ShouldHaveCountOf(1);
            Resolve.ResolvedPackages.First().Package.ShouldBeNull();
        }

        [Test]
        public void resolution_fails()
        {
            Resolve.IsSuccess.ShouldBeFalse();
        }
    }

    public class cyclic_dependency_in_packages : dependency_manager_context
    {
        public cyclic_dependency_in_packages()
        {
            given_system_package("evil-1.0.0", "depends: evil");

            given_dependency("depends: evil");

            when_resolving_packages();
        }

        [Test]
        public void package_is_present()
        {
            Resolve.ResolvedPackages.Count().ShouldBe(1);
            Resolve.ResolvedPackages.First()
                    .Check(x => x.Package.Name.ShouldBe("evil"))
                    .Check(x => x.Package.Version.ShouldBe(new Version("1.0.0")));
        }

        [Test]
        public void resolve_is_successful()
        {
            Resolve.IsSuccess.ShouldBeTrue();
        }
    }

    public class versions_in_conflict_and_dependency_override : dependency_manager_context
    {
        public versions_in_conflict_and_dependency_override()
        {
            given_project_package("sauron-1.0.0");
            given_project_package("sauron-1.1.0");
            given_project_package("rings-of-power-1.0.0", "depends: sauron = 1.0.0");
            given_project_package("one-ring-to-rule-them-all-1.0.0", "depends: sauron = 1.1.0");
            given_project_package("tolkien-1.0.0", "depends: rings-of-power", "depends: one-ring-to-rule-them-all");

            given_dependency("depends: tolkien");
            given_dependency("depends: sauron = 1.0.0");

            when_resolving_packages();
        }

        [Test]
        public void local_declaration_overrides_package_dependency()
        {
            Resolve.IsSuccess.ShouldBeTrue();
            Resolve.ResolvedPackages.Count().ShouldBe(4);
            Resolve.ResolvedPackages.ToLookup(x => x.Package.Name)["sauron"]
                    .ShouldHaveCountOf(1)
                    .First().Package.Version.ShouldBe(new Version(1, 0, 0));
        }
    }

    public class when_versions_are_in_conflict : dependency_manager_context
    {
        public when_versions_are_in_conflict()
        {
            given_project_package("sauron-1.0.0");
            given_project_package("sauron-1.1.0");
            given_project_package("rings-of-power-1.0.0", "depends: sauron = 1.0.0");
            given_project_package("one-ring-to-rule-them-all-1.0.0", "depends: sauron = 1.1.0");
            given_project_package("tolkien-1.0.0", "depends: rings-of-power", "depends: one-ring-to-rule-them-all");

            given_dependency("depends: tolkien");

            when_resolving_packages();
        }

        [Test]
        public void the_resolving_fails()
        {
            Resolve.IsSuccess.ShouldBeFalse();
            Resolve.ResolvedPackages.Count().ShouldBe(5);
            Resolve.ResolvedPackages.ToLookup(x => x.Package.Name)["sauron"].Count().ShouldBe(2);
        }
    }

    public class dependencies_in_conflict_wtith_local_definition : dependency_manager_context
    {
        public dependencies_in_conflict_wtith_local_definition()
        {
            given_project_package("log4net-1.0.0");

            given_remote1_package("castle.windsor-2.5.1", "depends: castle.core = 2.5.1");
            given_remote1_package("castle.windsor-2.1.1", "depends: castle.core");
            given_remote1_package("castle.core-2.5.1");
            given_remote1_package("castle.dynamicproxy-2.1.0", "depends: castle.core = 1.1.0");
            given_remote1_package("castle.dynamicproxy-2.2.0", "depends: castle.core = 1.2.0");
            given_remote1_package("castle.core-1.1.0", "depends: log4net");
            given_remote1_package("castle.core-1.2.0", "depends: log4net", "depends: NLog <= 1.0");
            given_remote1_package("castle.core-2.5.1");
            given_remote1_package("NHibernate.Core", "depends: log4net = 1.2.1");


            given_dependency("depends: castle.windsor = 2.1");
            given_dependency("depends: castle.core = 1.1");

            when_resolving_packages();
        }

        [Test]
        public void local_choice_overrides()
        {
            Resolve.ResolvedPackages.Where(x => x.PackageName == "castle.core")
                    .ShouldHaveCountOf(1)
                    .First().Package.Version.ShouldBe("1.1.0".ToVersion());
        }
    }

    public class resolving_package_from_remote_repository : dependency_manager_context
    {
        public resolving_package_from_remote_repository()
        {
            given_remote1_package("rings-of-power-1.0.0");
            given_dependency("depends: rings-of-power");

            when_resolving_packages();
        }

        [Test]
        public void dependency_on_remote_package_is_resolved()
        {
            Resolve.IsSuccess.ShouldBeTrue();
            Resolve.ResolvedPackages.First().Package.ShouldNotBeNull()
                    .Source.ShouldBe(RemoteRepository);
        }
    }

    public class resolving_package_from_system_repository : dependency_manager_context
    {
        public resolving_package_from_system_repository()
        {
            given_system_package("rings-of-power-1.0.0");
            given_dependency("depends: rings-of-power");

            when_resolving_packages();
        }

        [Test]
        public void system_package_is_resolved()
        {
            Resolve.IsSuccess.ShouldBeTrue();
            Resolve.ResolvedPackages.First().Package.ShouldNotBeNull()
                    .Source.ShouldBe(SystemRepository);
        }
    }

    public class resolvig_package_existing_in_local_and_remote : dependency_manager_context
    {
        public resolvig_package_existing_in_local_and_remote()
        {
            given_remote1_package("rings-of-power-1.1.0");
            given_project_package("rings-of-power-1.0.0");
            given_dependency("depends: rings-of-power");

            when_resolving_packages();
        }

        [Test]
        public void finds_highest_version_number_across_repositories()
        {
            Resolve.IsSuccess.ShouldBeTrue();
            var dependency = Resolve.ResolvedPackages.First();

            dependency.Package.ShouldNotBeNull()
                    .Source.ShouldBe(RemoteRepository);
            dependency.Package.Version.ShouldBe(new Version(1, 1, 0));
        }
    }

    public class resolving_package_existing_in_local : dependency_manager_context
    {
        public resolving_package_existing_in_local()
        {
            given_project_package("rings-of-power-1.0.0");
            given_dependency("depends: rings-of-power");

            when_resolving_packages();
        }

        [Test]
        public void package_is_resolved()
        {
            Resolve.IsSuccess.ShouldBeTrue();
            Resolve.ResolvedPackages.ShouldHaveCountOf(1);
        }
    }

    public class resolving_latest_package_in_system_with_outdated_remote_version : dependency_manager_context
    {
        public resolving_latest_package_in_system_with_outdated_remote_version()
        {
            given_system_package("one-ring-1.0.0.1");
            given_remote1_package("one-ring-1.0.0.0");

            given_dependency("depends: one-ring");

            when_resolving_packages();
        }

        [Test]
        public void resolve_is_successful()
        {
            Resolve.IsSuccess.ShouldBeTrue();
        }

        [Test]
        public void the_package_is_installed_from_system()
        {
            Resolve.ResolvedPackages.ShouldHaveCountOf(1)
                    .First()
                    .Check(x => x.Package.Source.ShouldBe(SystemRepository))
                    .Check(x => x.Package.Version.ShouldBe(new Version("1.0.0.1")));
        }
    }

    public class when_overriding_dependency : dependency_manager_context
    {
        public when_overriding_dependency()
        {
            given_remote1_package("one-ring-1.0.0");
            given_remote1_package("sauron-1.0.0", "depends: ring-of-power");

            given_project_package("minas-tirith-1.0.0");


            given_dependency("depends: sauron");
            given_dependency("depends: fangorn");


            given_dependency_override("ring-of-power", "one-ring");
            given_dependency_override("fangorn", "minas-tirith");

            when_resolving_packages();
        }

        [Test]
        public void dependencies_in_dependency_chain_are_overridden()
        {
            Resolve.ResolvedPackages.Where(x => x.PackageName == "one-ring").FirstOrDefault()
                    .ShouldNotBeNull()
                    .Package.Name.ShouldBe("one-ring");
        }

        [Test]
        public void locally_declared_dependency_is_overrridden()
        {
            Resolve.ResolvedPackages.Where(x => x.PackageName == "minas-tirith").FirstOrDefault()
                    .ShouldNotBeNull()
                    .Package.Name.ShouldBe("minas-tirith");
        }

        [Test]
        public void originally_declared_dependency_in_dependency_chain_is_not_resolved()
        {
            Resolve.ResolvedPackages.Where(x => x.PackageName == "ring-of-power")
                    .ShouldHaveCountOf(0);
        }

        [Test]
        public void originally_locally_declared_dependency_is_not_resolved()
        {
            Resolve.ResolvedPackages.Where(x => x.PackageName == "fangorn")
                    .ShouldHaveCountOf(0);
        }

        [Test]
        public void resolution_is_successfull()
        {
            Resolve.IsSuccess.ShouldBeTrue();
        }
    }

    namespace context
    {
        public abstract class dependency_manager_context : OpenWrap.Testing.context
        {
            protected InMemoryRepository CurrentDirectoryRepository;
            protected PackageDescriptor DependencyDescriptor;
            protected InMemoryRepository ProjectRepository;
            protected InMemoryRepository RemoteRepository;
            protected DependencyResolutionResult Resolve;
            protected InMemoryRepository SystemRepository;

            public dependency_manager_context()
            {
                DependencyDescriptor = new PackageDescriptor
                {
                        Name = "test",
                        Version = new Version("1.0")
                };
                ProjectRepository = new InMemoryRepository("Local repository");
                SystemRepository = new InMemoryRepository("System repository");
                RemoteRepository = new InMemoryRepository("Remote repository");
                CurrentDirectoryRepository = new InMemoryRepository("Current repository");
            }

            protected void given_current_directory_package(string name, params string[] dependencies)
            {
                Add(CurrentDirectoryRepository, name, dependencies);
            }

            protected void given_dependency(string dependency)
            {
                new DependsParser().Parse(dependency, DependencyDescriptor);
            }

            protected void given_dependency_override(string from, string to)
            {
                DependencyDescriptor.Overrides.Add(new PackageNameOverride(from, to));
            }

            protected void given_project_package(string name, params string[] dependencies)
            {
                Add(ProjectRepository, name, dependencies);
            }

            protected void given_remote1_package(string name, params string[] dependencies)
            {
                Add(RemoteRepository, name, dependencies);
            }

            protected void given_system_package(string name, params string[] dependencies)
            {
                Add(SystemRepository, name, dependencies);
            }

            protected void when_resolving_packages()
            {
                Resolve = new PackageResolver()
                        .TryResolveDependencies(DependencyDescriptor,
                                                new[]
                                                {
                                                        CurrentDirectoryRepository,
                                                        ProjectRepository,
                                                        SystemRepository,
                                                        RemoteRepository
                                                });
            }

            void Add(InMemoryRepository repository, string name, string[] dependencies)
            {
                var package = new InMemoryPackage
                {
                        Name = PackageNameUtility.GetName(name),
                        Version = PackageNameUtility.GetVersion(name),
                        Source = repository,
                        Dependencies = dependencies.SelectMany(x => DependsParser.ParseDependsInstruction(x).Dependencies)
                                .ToList()
                };
                repository.Packages.Add(package);
            }
        }
    }
}