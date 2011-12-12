using System.Linq;
using System.Xml.Linq;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenWrap;
using OpenWrap.IO.Packaging;
using OpenWrap.PackageModel;
using OpenWrap.Repositories.FileSystem;
using OpenWrap.Runtime;
using OpenWrap.Services;
using OpenWrap.Tests.Commands;

namespace Tests.Repositories.context
{
    public abstract class indexed_folder_repository : OpenWrap.Testing.context
    {
        protected IndexedFolderRepository Repository { get; set; }
        protected InMemoryEnvironment Environment { get; set; }
        protected InMemoryFileSystem FileSystem { get; set; }
        protected ILookup<string, IPackageInfo> PackagesByName { get; set; }
        protected IPackageInfo FoundPackage { get; set; }
        protected XDocument IndexDocument { get { return ((FileSystemNavigator)Repository.Navigator).IndexDocument; } }

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

            ServiceLocator.RegisterService<IFileSystem>(FileSystem);

            Environment = new InMemoryEnvironment(FileSystem.GetDirectory(currentDirectory),
                                                  FileSystem.GetDirectory(DefaultInstallationPaths.ConfigurationDirectory));
            ServiceLocator.RegisterService<IEnvironment>(Environment);
        }

        protected InMemoryFile Package(string wrapName, string version, params string[] wrapdescLines)
        {
            var file = new InMemoryFile(wrapName + "-" + version + ".wrap");
            Packager.NewWithDescriptor(file, wrapName, version, wrapdescLines);
            return file;
        }

        protected void when_publishing_package(InMemoryFile package)
        {
            using (var stream = package.OpenRead())
            using(var publisher = Repository.Publisher())
                publisher.Publish(package.Name, stream);
        }

        protected void when_nuking_package(string name, string version)
        {
            Repository.Nuke(
                Repository.PackagesByName[name]
                    .Where(x => x.Version == version.ToSemVer())
                    .First());
            Repository.RefreshPackages();
        }

        protected void given_published_package(string packageName, string packageVersion)
        {
            when_publishing_package(Package(packageName, packageVersion));
        }
    }
}