using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using OpenWrap.Dependencies;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystem.InMemory;
using OpenWrap.Repositories;
using OpenWrap.Services;
using OpenWrap.Tests.Commands;

namespace OpenWrap.Tests.Repositories
{
    namespace context
    {
        public class project_repository : Testing.context
        {
            protected CurrentDirectoryRepository Repository { get; set; }
            protected InMemoryEnvironment Environment { get; set; }
            protected InMemoryFileSystem FileSystem { get; set; }
            protected ILookup<string, IPackageInfo> PackagesByName { get; set; }
            protected IPackageInfo FoundPackage { get; set; }

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

            protected void when_getting_package_names()
            {
                PackagesByName = Repository.PackagesByName;
            }

            protected InMemoryFile Package(string wrapName)
            {
                var newFile = new InMemoryFile(wrapName + ".wrap");
                using (var stream = newFile.OpenWrite())
                using (var zipFile = new ZipFile(stream))
                {
                    zipFile.BeginUpdate();
                    var fileName = wrapName + ".wrapdesc";
                    zipFile.NameTransform = new ZipNameTransform(fileName);
                    zipFile.Add(new ZipEntry(fileName) { Size = 0, CompressedSize = 0 });
                    zipFile.CommitUpdate();

                }
                return newFile;
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
                new WrapDependencyParser().Parse(dependency, dep);

                FoundPackage = Repository.Find(dep.Dependencies.First());
            }
        }
    }
}
