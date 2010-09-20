using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using OpenRasta.Wrap.Tests.Dependencies.context;
using OpenWrap.Dependencies;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystem.InMemory;
using OpenWrap.Repositories;
using OpenWrap.Services;
using OpenWrap.Testing;
using OpenWrap.Tests.Commands;

namespace OpenWrap.Tests.Repositories
{
    public class when_publishing_a_package : context.indexed_folder_repository
    {
        public when_publishing_a_package()
        {
            given_file_system(@"c:\tmp");
            given_indexed_repository(@"c:\tmp\repository");

            when_publishing_package(Package("isengard", "2.1", "depends: saruman"));
        }

        [Test]
        public void index_file_exists()
        {
            Repository.IndexDocument.ShouldNotBeNull();
        }
        [Test]
        public void index_file_is_not_empty()
        {
            IndexDocument.Document.ShouldNotBeNull();
        }
        [Test]
        public void index_file_contains_package()
        {
            var package = IndexDocument.Document.Descendants("wrap").FirstOrDefault();
            package.ShouldNotBeNull();
            package.Attribute("name").ShouldNotBeNull().Value.ShouldBe("isengard");
            package.Attribute("version").ShouldNotBeNull().Value.ShouldBe("2.1");
            var link = package.Descendants("link").FirstOrDefault().ShouldNotBeNull();
            link.Attribute("href").ShouldNotBeNull().Value.ShouldNotBeNull().ShouldContain("isengard-2.1.wrap");
        }
        [Test]
        public void index_file_contains_package_dependencies()
        {
            Repository.PackagesByName["isengard"].First()
                    .Dependencies.ShouldHaveCountOf(1)
                    .First().Name.ShouldBe("saruman");
        }
        [Test]
        public void package_is_accessible()
        {
            Repository.PackagesByName["isengard"].FirstOrDefault().ShouldNotBeNull()
                .Load().ShouldNotBeNull()
                .OpenStream().ReadToEnd().Length.ShouldBeGreaterThan(0);
        }
    }

    namespace context
    {
        public abstract class indexed_folder_repository : Testing.context
        {
            protected IndexedFolderRepository Repository { get; set; }
            protected InMemoryEnvironment Environment { get; set; }
            protected InMemoryFileSystem FileSystem { get; set; }
            protected ILookup<string, IPackageInfo> PackagesByName { get; set; }
            protected IPackageInfo FoundPackage { get; set; }
            protected XDocument IndexDocument { get { return ((NetworkShareNavigator)Repository.Navigator).IndexDocument; } }

            protected void given_indexed_repository(string path)
            {
                Repository = new IndexedFolderRepository("local", FileSystem.GetDirectory(path));
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

            protected void when_getting_package_names()
            {
                PackagesByName = Repository.PackagesByName;
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

            protected InMemoryFile Package(string wrapName, string version, params string[] wrapdescLines)
            {
                var file = new InMemoryFile(wrapName + "-" + version + ".wrap");
                PackageBuilder.New(file, wrapName, version, wrapdescLines);
                return file;
            }

            protected void when_publishing_package(InMemoryFile package)
            {
                using (var stream = package.OpenRead())
                    Repository.Publish(package.Name, stream);
            }
        }
    }
}
