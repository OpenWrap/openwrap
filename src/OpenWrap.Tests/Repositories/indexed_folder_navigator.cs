using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using OpenRasta.Wrap.Tests.Dependencies.context;
using OpenWrap.Dependencies;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.InMemory;
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

    public class when_nuking_a_package : context.indexed_folder_repository
    {
        public when_nuking_a_package()
        {
            given_file_system(@"c:\nuke");
            given_indexed_repository(@"c:\nuke\repository");
            given_published_package("pharrell", "1.0.0.0");
            when_nuking_package("pharrell", "1.0.0.0");
        }

        [Test]
        public void index_document_contains_nuked_attribute()
        {
            (from XElement node in IndexDocument.Descendants("wrap")
             where node.Attribute("name").Value.Equals("pharrell") &&
                   node.Attribute("version").Value.Equals("1.0.0.0") &&
                   node.Attribute("nuked").Value.Equals("true")
             select node)
            .ShouldHaveCountOf(1);
        }
        [Test]
        public void returned_packageinfo_is_marked_as_nuked()
        {
            Repository.PackagesByName["pharrell"]
                .Where(p => p.Version.ToString().Equals("1.0.0.0"))
                .FirstOrDefault()
                .ShouldNotBeNull()
                .Nuked
                .ShouldBeTrue();

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


            protected void given_file_system(string currentDirectory)
            {
                FileSystem = new InMemoryFileSystem()
                {
                    CurrentDirectory = currentDirectory
                };

                Services.Services.RegisterService<IFileSystem>(FileSystem);

                Environment = new InMemoryEnvironment(FileSystem.GetDirectory(currentDirectory),
                    FileSystem.GetDirectory(InstallationPaths.ConfigurationDirectory));
                Services.Services.RegisterService<IEnvironment>(Environment);
            }

            protected InMemoryFile Package(string wrapName, string version, params string[] wrapdescLines)
            {
                var file = new InMemoryFile(wrapName + "-" + version + ".wrap");
                PackageBuilder.NewWithDescriptor(file, wrapName, version, wrapdescLines);
                return file;
            }

            protected void when_publishing_package(InMemoryFile package)
            {
                using (var stream = package.OpenRead())
                    Repository.Publish(package.Name, stream);
            }

            protected void when_nuking_package(string name, string version)
            {
                Repository.Nuke(
                    Repository.PackagesByName[name]
                    .Where(x => x.Version.ToString().Equals(version))
                    .First());
                Repository.RefreshPackages();
            }

            protected void given_published_package(string packageName, string packageVersion)
            {
                when_publishing_package(Package(packageName, packageVersion));
            }
        }
    }
}
