using System;
using System.IO;
using System.Linq;
using current_directory_specifications.context;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystem.InMemory;
using OpenRasta.Wrap.Tests.Dependencies.context;
using OpenWrap;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;
using OpenWrap.Services;
using OpenWrap.Testing;
using OpenWrap.Tests.Commands;

namespace current_directory_specifications
{
    public class when_reading_packages_by_name : current_directory_repository
    {
        public when_reading_packages_by_name()
        {
            given_packages_in_directory(@"c:\mordor\", Package("isenmouthe", "1.0.0"), Package("gorgoroth", "2.0.0"));
            given_current_folder_repository();
            when_getting_package_names();
        }

        [Test]
        public void the_packages_are_available_by_name()
        {
            PackagesByName.Contains("isenmouthe").ShouldBeTrue();
            PackagesByName.Contains("gorgoroth").ShouldBeTrue();
        }
    }

    public class publishing_package : current_directory_repository
    {
        public publishing_package()
        {
            given_current_folder_repository();
        }

        [Test]
        public void attempting_publish_results_in_error()
        {
            Executing(() => Repository.Publish("isengard", new MemoryStream()))
                    .ShouldThrow<NotSupportedException>();
        }

        [Test]
        public void publish_is_disabled()
        {
            Repository.CanPublish.ShouldBeFalse();
        }
    }

    public class finding_a_package : current_directory_repository
    {
        public finding_a_package()
        {
            given_packages_in_directory(@"c:\mordor\", Package("isenmouthe", "1.0.0"));
            given_current_folder_repository();
            when_finding_packages("depends: isenmouthe");
        }

        [Test]
        public void the_package_is_found()
        {
            FoundPackage.ShouldNotBeNull()
                    .Name.ShouldBe("isenmouthe");
        }
    }

    namespace context
    {
        public abstract class current_directory_repository : OpenWrap.Testing.context
        {
            protected InMemoryEnvironment Environment { get; set; }
            protected InMemoryFileSystem FileSystem { get; set; }
            protected IPackageInfo FoundPackage { get; set; }
            protected ILookup<string, IPackageInfo> PackagesByName { get; set; }
            protected CurrentDirectoryRepository Repository { get; set; }

            protected InMemoryFile Package(string wrapName, string version)
            {
                var file = new InMemoryFile(wrapName + "-" + version + ".wrap");
                PackageBuilder.New(file, wrapName, version);
                return file;
            }

            protected void given_current_folder_repository()
            {
                Repository = new CurrentDirectoryRepository();
            }

            protected void given_file_system(string currentDirectory, params InMemoryDirectory[] directories)
            {
                FileSystem = new InMemoryFileSystem(directories)
                {
                        CurrentDirectory = currentDirectory
                };
                WrapServices.RegisterService<IFileSystem>(FileSystem);

                Environment = new InMemoryEnvironment(FileSystem.GetDirectory(currentDirectory),
                                                      FileSystem.GetDirectory(InstallationPaths.ConfigurationDirectory));
                WrapServices.RegisterService<IEnvironment>(Environment);
            }

            protected void given_packages_in_directory(string currentDirectory, params InMemoryFile[] packages)
            {
                given_file_system(
                        currentDirectory,
                        new InMemoryDirectory(currentDirectory,
                                              packages));
            }

            protected void when_finding_packages(string dependency)
            {
                var dep = new WrapDescriptor();
                new DependsParser().Parse(dependency, dep);

                FoundPackage = Repository.Find(dep.Dependencies.First());
            }

            protected void when_getting_package_names()
            {
                PackagesByName = Repository.PackagesByName;
            }
        }
    }
}