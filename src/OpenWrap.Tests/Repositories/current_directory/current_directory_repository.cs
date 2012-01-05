using System;
using System.Linq;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenWrap;
using OpenWrap.IO.Packaging;
using OpenWrap.PackageModel;
using OpenWrap.PackageModel.Parsers;
using OpenWrap.Repositories;
using OpenWrap.Runtime;
using OpenWrap.Services;
using OpenWrap.Tests.Commands;

namespace Tests.Repositories
{
    namespace context
    {
        public abstract class current_directory_repository : OpenWrap.Testing.context
        {
            protected InMemoryEnvironment Environment { get; set; }
            protected InMemoryFileSystem FileSystem { get; set; }
            protected IPackageInfo FoundPackage { get; set; }
            protected ILookup<string, IPackageInfo> PackagesByName { get; set; }
            protected CurrentDirectoryRepository Repository { get; set; }

            protected void given_current_folder_repository()
            {
                Repository = new CurrentDirectoryRepository();
            }

            protected void given_file_system(string currentDirectory)
            {
                FileSystem = new InMemoryFileSystem
                {
                        CurrentDirectory = currentDirectory
                };
                ServiceLocator.RegisterService<IFileSystem>(FileSystem);

                Environment = new InMemoryEnvironment(FileSystem.GetDirectory(currentDirectory),
                                                      FileSystem.GetDirectory(DefaultInstallationPaths.ConfigurationDirectory));
                ServiceLocator.RegisterService<IEnvironment>(Environment);
            }

            protected void given_packages(params IFile[] packages)
            {
                Repository.RefreshPackages();
            }

            protected void when_finding_packages(string dependency)
            {
                var dep = new PackageDescriptor();
                new DependsParser().Parse(dependency, dep);

                FoundPackage = Repository.PackagesByName.FindAll(dep.Dependencies.First()).FirstOrDefault();
            }

            protected void when_getting_package_names()
            {
                PackagesByName = Repository.PackagesByName;
            }

            protected IFile Package(string wrapName, string version)
            {
                var file = FileSystem.GetFile(wrapName + "-" + version + ".wrap").MustExist();
                return Packager.NewWithDescriptor(file, wrapName, version);
            }
        }
    }
}