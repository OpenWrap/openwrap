using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenWrap.Collections;
using OpenWrap.Commands.Cli;
using OpenWrap.Configuration;
using OpenFileSystem.IO;
using OpenWrap.IO.Packaging;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.DependencyResolvers;
using OpenWrap.PackageManagement.Deployers;
using OpenWrap.PackageManagement.Exporters;
using OpenWrap.PackageManagement.Packages;
using OpenWrap.PackageModel;
using OpenWrap.PackageModel.Parsers;
using OpenWrap.PackageModel.Serialization;
using OpenWrap.Repositories;
using OpenWrap.Runtime;
using OpenWrap.Testing;
using OpenWrap.Tests.Commands;

namespace OpenWrap.Commands.contexts
{
    public abstract class command : OpenWrap.Testing.context
    {
        protected CommandRepository Commands;
        protected InMemoryEnvironment Environment;
        protected IFileSystem FileSystem;

        protected command()
        {
            Services.ServiceLocator.Clear();
            var currentDirectory = System.Environment.CurrentDirectory;
            FileSystem = given_file_system(currentDirectory);
            Environment = new InMemoryEnvironment(
                    FileSystem.GetDirectory(currentDirectory),
                    FileSystem.GetDirectory(DefaultInstallationPaths.ConfigurationDirectory));
            Environment.DescriptorFile.MustExist();
            Services.ServiceLocator.RegisterService<IFileSystem>(FileSystem);
            Services.ServiceLocator.RegisterService<IEnvironment>(Environment);
            Services.ServiceLocator.RegisterService<IPackageResolver>(new ExhaustiveResolver());
            Services.ServiceLocator.TryRegisterService<IPackageDeployer>(() => new DefaultPackageDeployer());
            Services.ServiceLocator.TryRegisterService<IPackageExporter>(() => new DefaultPackageExporter());
            Services.ServiceLocator.RegisterService<ICommandRepository>(Commands);
            
            Services.ServiceLocator.TryRegisterService<IPackageManager>(PackageManagerFactory());
            
            Services.ServiceLocator.RegisterService<IConfigurationManager>(new DefaultConfigurationManager(Environment.ConfigurationDirectory));
        }

        protected virtual Func<IPackageManager> PackageManagerFactory()
        {
            return () => new DefaultPackageManager(
                                 Services.ServiceLocator.GetService<IPackageDeployer>(),
                                 Services.ServiceLocator.GetService<IPackageResolver>());
        }

        protected virtual IFileSystem given_file_system(string currentDirectory)
        {
            return new InMemoryFileSystem() { CurrentDirectory = currentDirectory };
        }

        protected void given_dependency(string scope, string dependency)
        {
            new DependsParser().Parse(dependency, Environment.GetOrCreateScopedDescriptor(scope).Value);
        }
        
        protected void given_dependency(string dependency)
        {
            given_dependency(string.Empty, dependency);

        }


        protected void given_project_package(string name, string version, params string[] dependencies)
        {
            given_project_repository(new InMemoryRepository("Project repository"));
            AddPackage(Environment.ProjectRepository, name, version, dependencies);
        }

        protected void given_project_repository()
        {
            given_project_repository(new InMemoryRepository("Project repository"));
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
            // note Version is a version type because of overload resolution...
            AddPackage(Environment.RemoteRepository, name, version.ToString(), dependencies);
        }

        protected void given_remote_package(string repositoryName, string name, Version version, params string[] dependencies)
        {
            AddPackage(Environment.RemoteRepositories.First(x=>x.Name == repositoryName), name, version.ToString(), dependencies);
        }

        protected void given_system_package(string name, string version, params string[] dependencies)
        {
            AddPackage(Environment.SystemRepository, name, version, dependencies);
        }

        static void AddPackage(IPackageRepository repository, string name, string version, string[] dependencies)
        {
            if (repository is InMemoryRepository)
            {
                ((InMemoryRepository)repository).Packages.Add(new InMemoryPackage
                {
                        Name = name,
                        Source = repository,
                        Version = version.ToVersion(),
                        Dependencies = dependencies.SelectMany(x => DependsParser.ParseDependsInstruction(x).Dependencies).ToList()
                });
                return;
            }
            var packageFileName = name + "-" + version + ".wrap";
            var packageStream = Packager.NewWithDescriptor(new InMemoryFile(packageFileName), name, version.ToString(), dependencies).OpenRead();
            using (var readStream = packageStream)
            using (var publisher = ((ISupportPublishing)repository).Publisher())
                publisher.Publish(packageFileName, readStream);
        }

        protected void given_currentdirectory_package(string packageName, string version, params string[] dependencies)
        {
            given_currentdirectory_package(packageName, new Version(version), dependencies);
        }

        protected void given_currentdirectory_package(string packageName, Version version, params string[] dependencies)
        {
            if (Environment.CurrentDirectoryRepository is InMemoryRepository)
                AddPackage(Environment.CurrentDirectoryRepository, packageName, version.ToString(), dependencies);
            else
            {
                var localFile = Environment.CurrentDirectory.GetFile(PackageNameUtility.PackageFileName(packageName, version.ToString())).MustExist();
                Packager.NewWithDescriptor(localFile, packageName, version.ToString(), dependencies);
                
            }
        }

        protected void given_remote_configuration(RemoteRepositories remoteRepositories)
        {
            Services.ServiceLocator.GetService<IConfigurationManager>()
                    .Save(Configurations.Addresses.RemoteRepositories, remoteRepositories);
        }

        protected void given_file(string filePath, Stream stream)
        {

            var file = FileSystem.GetFile(filePath);
            using(var newFile = file.OpenWrite())
            {
                StreamExtensions.CopyTo(stream, newFile);
            }
        }

        protected void given_remote_repository(string remoteName)
        {
            Environment.RemoteRepositories.Add(new InMemoryRepository(remoteName));
        }

        protected void given_current_directory(string currentDirectory)
        {
            if (FileSystem is InMemoryFileSystem)
                ((InMemoryFileSystem)FileSystem).CurrentDirectory = currentDirectory;
            Environment.CurrentDirectory = FileSystem.GetDirectory(currentDirectory);

        }

        protected void given_default_descriptor(PackageDescriptor packageDescriptor)
        {
            Environment.Descriptor = packageDescriptor;
        }
    }

    public abstract class command_context<T> : command where T : ICommand
    {
        protected AttributeBasedCommandDescriptor<T> Command;
        protected List<ICommandOutput> Results;

        public command_context()
        {
            Command = new AttributeBasedCommandDescriptor<T>();
            Commands = new CommandRepository { Command };

            
        }

        protected virtual void when_executing_command(params string[] parameters)
        {
            foreach (var descriptor in Environment.ScopedDescriptors.Values)
                descriptor.Save();
            var allParams = new[] { Command.Noun, Command.Verb }.Concat(parameters);
            Results = new CommandLineProcessor(Commands).Execute(allParams).ToList();
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

        public IPackageDescriptor WrittenDescriptor(string scope = null)
        {
            scope = scope ?? string.Empty;
            return new PackageDescriptorReaderWriter().Read(Environment.ScopedDescriptors[scope].File);
        }
    }
}