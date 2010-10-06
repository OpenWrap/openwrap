using System;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystem.InMemory;
using OpenRasta.Wrap.Tests.Dependencies.context;
using OpenWrap;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace OpenRasta.Wrap.Tests.Dependencies
{
    public class when_resolving_unavailable_dependencies : dependency_manager_context
    {
        public when_resolving_unavailable_dependencies()
        {
            given_dependency("depends: rings-of-power");

            when_resolving_packages();
        }
        [Test]
        public void resolution_fails()
        {
            Resolve.IsSuccess.ShouldBeFalse();
        }
        [Test]
        public void dependency_has_no_package()
        {
            Resolve.Dependencies.ShouldHaveCountOf(1);
            Resolve.Dependencies.First().Package.ShouldBeNull();
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
        public void resolve_is_successful()
        {
            Resolve.IsSuccess.ShouldBeTrue();
        }
        [Test]
        public void package_is_present()
        {
            Resolve.Dependencies.Count().ShouldBe(1);
            Resolve.Dependencies.First()
                    .Check(x => x.Package.Name.ShouldBe("evil"))
                    .Check(x => x.Package.Version.ShouldBe(new Version("1.0.0")));
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
            Resolve.Dependencies.Count().ShouldBe(4);
            Resolve.Dependencies.ToLookup(x => x.Package.Name)["sauron"]
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
            Resolve.Dependencies.Count().ShouldBe(5);
            Resolve.Dependencies.ToLookup(x => x.Package.Name)["sauron"].Count().ShouldBe(2);
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
            Resolve.Dependencies.First().Package.ShouldNotBeNull()
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
            Resolve.Dependencies.First().Package.ShouldNotBeNull()
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
            var dependency = Resolve.Dependencies.First();

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
            Resolve.Dependencies.ShouldHaveCountOf(1);
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
            Resolve.Dependencies.ShouldHaveCountOf(1)
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
        public void resolution_is_successfull()
        {
            Resolve.IsSuccess.ShouldBeTrue();
        }
        [Test]
        public void originally_locally_declared_dependency_is_not_resolved()
        {
            Resolve.Dependencies.Where(x => x.Dependency.Name == "fangorn")
                    .ShouldHaveCountOf(0);

        }
        [Test]
        public void locally_declared_dependency_is_overrridden()
        {
            Resolve.Dependencies.Where(x => x.Dependency.Name == "minas-tirith").FirstOrDefault()
                    .ShouldNotBeNull()
                    .Package.Name.ShouldBe("minas-tirith");
        }

        [Test]
        public void dependencies_in_dependency_chain_are_overridden()
        {
            Resolve.Dependencies.Where(x => x.Dependency.Name == "one-ring").FirstOrDefault()
                    .ShouldNotBeNull()
                    .Package.Name.ShouldBe("one-ring");
        }
        [Test]
        public void originally_declared_dependency_in_dependency_chain_is_not_resolved()
        {
            Resolve.Dependencies.Where(x => x.Dependency.Name == "ring-of-power")
                    .ShouldHaveCountOf(0);

        }
    }

    namespace context
    {
        public abstract class dependency_manager_context : OpenWrap.Testing.context
        {
            protected WrapDescriptor DependencyDescriptor;
            protected InMemoryRepository ProjectRepository;
            protected InMemoryRepository RemoteRepository;
            protected DependencyResolutionResult Resolve;
            protected InMemoryRepository SystemRepository;

            public dependency_manager_context()
            {

                DependencyDescriptor = new WrapDescriptor
                {
                    Name = "test",
                    Version = new Version("1.0")
                };
                ProjectRepository = new InMemoryRepository("Local repository");
                SystemRepository = new InMemoryRepository("System repository");
                RemoteRepository = new InMemoryRepository("Remote repository");
            }

            protected void given_dependency(string dependency)
            {
                new DependsParser().Parse(dependency, DependencyDescriptor);
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
                Resolve = new PackageManager().TryResolveDependencies(DependencyDescriptor,
                                                                      new[]
                                                                      {
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

            protected void given_dependency_override(string from, string to)
            {
                DependencyDescriptor.Overrides.Add(new PackageNameOverride(from, to));            
            }
        }

        public static class PackageBuilder
        {

            public static IFile New(IFile wrapFile, string name, string version, params string[] descriptorLines)
            {
                //var wrapFile = new InMemoryFile(name + "-" + version + ".wrap");
                using (var wrapStream = wrapFile.OpenWrite())
                using (var zipFile = new ZipOutputStream(wrapStream))
                {
                    var nameTransform = new ZipNameTransform();

                    zipFile.PutNextEntry(new ZipEntry(name + ".wrapdesc"));

                    var descriptorContent = descriptorLines.Any()
                                                    ? string.Join("\r\n", descriptorLines)
                                                    : " ";
                    
                    zipFile.Write(Encoding.UTF8.GetBytes(descriptorContent));

                    var versionEntry = new ZipEntry("version");
                    zipFile.PutNextEntry(versionEntry);

                    var versionData = Encoding.UTF8.GetBytes(version);
                    zipFile.Write(versionData);
                    zipFile.Finish();
                }
                return wrapFile;
            }
        }
    }
}
