using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenFileSystem.IO.FileSystem.InMemory;
using OpenRasta.Wrap.Tests.Dependencies.context;
using OpenWrap.Commands;
using OpenWrap.Commands.Wrap;
using OpenWrap.Configuration;
using OpenWrap.Configuration.remote_repositories;
using OpenWrap.Dependencies;
using OpenFileSystem.IO;
using OpenWrap.Repositories;
using OpenWrap.Services;
using OpenWrap.Testing;

namespace OpenWrap.Tests.Commands.context
{
    public abstract class command_context<T> : Testing.context
        where T : ICommand
    {
        protected AttributeBasedCommandDescriptor<T> Command;
        protected CommandRepository Commands;
        protected InMemoryEnvironment Environment;
        protected List<ICommandOutput> Results;
        protected InMemoryFileSystem FileSystem;

        public command_context()
        {
            Command = new AttributeBasedCommandDescriptor<T>();
            Commands = new CommandRepository { Command };

            Services.Services.Clear();
            var currentDirectory = System.Environment.CurrentDirectory;
            FileSystem = new InMemoryFileSystem() { CurrentDirectory = currentDirectory };
            Environment = new InMemoryEnvironment(
                FileSystem.GetDirectory(currentDirectory),
                FileSystem.GetDirectory(InstallationPaths.ConfigurationDirectory));
            Services.Services.RegisterService<IFileSystem>(FileSystem);
            Services.Services.RegisterService<IEnvironment>(Environment);
            Services.Services.RegisterService<IPackageManager>(new PackageManager());
            Services.Services.RegisterService<ICommandRepository>(Commands);
            Services.Services.RegisterService<IConfigurationManager>(new ConfigurationManager(Environment.ConfigurationDirectory));
        }

        protected void given_dependency(string dependency)
        {
            new DependsParser().Parse(dependency, Environment.Descriptor);
        }

        protected void given_project_package(string name, Version version, params string[] dependencies)
        {
            given_project_repository(new InMemoryRepository("Project repository"));
            AddInMemoryPackage(Environment.ProjectRepository, name, version, dependencies);
        }

        protected void given_project_repository(IPackageRepository repository)
        {
            if (Environment.ProjectRepository == null)
                Environment.ProjectRepository = repository;
        }
        protected void given_current_directory_repository(CurrentDirectoryRepository repository)
        {
            Environment.CurrentDirectoryRepository = repository;
        }
        protected void given_remote_package(string name, Version version, params string[] dependencies)
        {
            AddInMemoryPackage(Environment.RemoteRepository, name, version, dependencies);
        }

        protected void given_system_package(string name, Version version, params string[] dependencies)
        {
            AddInMemoryPackage(Environment.SystemRepository, name, version, dependencies);
        }

        protected virtual void when_executing_command(params string[] parameters)
        {
            var allParams = new[] { Command.Noun, Command.Verb }.Concat(parameters);
            Results = new CommandLineProcessor(Commands).Execute(allParams).ToList();
        }

        protected static void AddInMemoryPackage(IPackageRepository repository, string name, Version version, string[] dependencies)
        {
            if (repository is InMemoryRepository)
            {
                ((InMemoryRepository)repository).Packages.Add(new InMemoryPackage
                {
                    Name = name,
                    Source = repository,
                    Version = version,
                    Dependencies = dependencies.SelectMany(x => DependsParser.ParseDependsInstruction(x).Dependencies).ToList()
                });
                return;
            }
            var packageFileName = name + "-" + version + ".wrap";
            var packageStream = PackageBuilder.NewWithDescriptor(new InMemoryFile(packageFileName), name, version.ToString(), dependencies).OpenRead();
            using (var readStream = packageStream)
                ((ISupportPublishing)repository).Publish(packageFileName, readStream);
        }

        protected void given_currentdirectory_package(string packageName, string version, params string[] dependencies)
        {
            given_currentdirectory_package(packageName, new Version(version), dependencies);
        }
        protected void given_currentdirectory_package(string packageName, Version version, params string[] dependencies)
        {
            if (Environment.CurrentDirectoryRepository is InMemoryRepository)
                AddInMemoryPackage(Environment.CurrentDirectoryRepository, packageName, version, dependencies);
            else
            {
                var localFile = Environment.CurrentDirectory.GetFile(PackageNameUtility.PacakgeFileName(packageName, version.ToString())).MustExist();
                PackageBuilder.NewWithDescriptor(localFile, packageName, version.ToString(), dependencies);
                
            }
        }

        protected void package_is_not_in_repository(IPackageRepository repository, string packageName, Version packageVersion)
        {
            (repository.PackagesByName.Contains(packageName)
                              ? repository.PackagesByName[packageName].FirstOrDefault(x => x.Version.Equals(packageVersion))
                              : null).ShouldBeNull();


        }
        protected void package_is_in_repository(IPackageRepository repository, string packageName, Version packageVersion)
        {
            repository.PackagesByName[packageName]
                .ShouldHaveCountOf(1)
                .First().Version.ShouldBe(packageVersion);
        }

        protected void given_remote_configuration(RemoteRepositories remoteRepositories)
        {
            Services.Services.GetService<IConfigurationManager>()
                    .Save(Configurations.Addresses.RemoteRepositories, remoteRepositories);
        }
        protected void given_file(string filePath, Stream stream)
         {

            var file = FileSystem.GetFile(filePath);
            using(var newFile = file.OpenWrite())
            {
                stream.CopyTo(newFile);
            }
         }
        protected void given_remote_repository(string remoteName)
        {
            Environment.RemoteRepositories.Add(new InMemoryRepository(remoteName));
        }

        protected void given_current_directory(string currentDirectory)
        {
            FileSystem.CurrentDirectory = currentDirectory;
            Environment.CurrentDirectory = FileSystem.GetDirectory(currentDirectory);

        }
    }
}