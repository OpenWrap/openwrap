using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenWrap;
using OpenWrap.Commands;
using OpenWrap.Commands.Cli;
using OpenWrap.Configuration;
using OpenFileSystem.IO;
using OpenWrap.Configuration.Remotes;
using OpenWrap.IO.Packaging;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.DependencyResolvers;
using OpenWrap.PackageManagement.Deployers;
using OpenWrap.PackageManagement.Exporters;
using OpenWrap.PackageManagement.Exporters.Assemblies;
using OpenWrap.PackageManagement.Exporters.Commands;
using OpenWrap.PackageManagement.Packages;
using OpenWrap.PackageModel;
using OpenWrap.PackageModel.Parsers;
using OpenWrap.PackageModel.Serialization;
using OpenWrap.Repositories;
using OpenWrap.Runtime;
using OpenWrap.Services;
using OpenWrap.Testing;
using OpenWrap.Tests.Commands;
using Tests.Repositories.manager;

namespace Tests
{
    public abstract class openwrap_context : OpenWrap.Testing.context
    {
        protected CommandRepository Commands;
        protected InMemoryEnvironment Environment;
        protected IFileSystem FileSystem;
        protected List<IRemoteRepositoryFactory> RemoteFactories;
        protected MemoryRepositoryFactory Factory;
        protected RemoteRepositories ConfiguredRemotes;
        protected List<IPackageRepository> RemoteRepositories;
        protected IConfigurationManager ConfigurationManager;

        protected openwrap_context()
        {
            // TODO: Review if we should use the Service registry?
            RemoteRepositories = new List<IPackageRepository>();
            ConfiguredRemotes = new RemoteRepositories();
            ServiceLocator.Clear();
            var currentDirectory = System.Environment.CurrentDirectory;
            FileSystem = given_file_system(currentDirectory);
            Environment = new InMemoryEnvironment(
                    FileSystem.GetDirectory(currentDirectory),
                    FileSystem.GetDirectory(DefaultInstallationPaths.ConfigurationDirectory));
            //Environment.DescriptorFile.MustExist();
            ServiceLocator.RegisterService<IFileSystem>(FileSystem);
            ServiceLocator.RegisterService<IEnvironment>(Environment);
            ServiceLocator.RegisterService<IPackageResolver>(new ExhaustiveResolver());
            ServiceLocator.TryRegisterService<IPackageDeployer>(() => new DefaultPackageDeployer());
            ServiceLocator.TryRegisterService<IPackageExporter>(() => new DefaultPackageExporter(new IExportProvider[]{
                    new DefaultAssemblyExporter(),
                    new CecilCommandExporter()
                }));
            ServiceLocator.RegisterService<ICommandRepository>(Commands);

            ServiceLocator.TryRegisterService<IPackageManager>(PackageManagerFactory());

            ServiceLocator.RegisterService<IConfigurationManager>(new DefaultConfigurationManager(Environment.ConfigurationDirectory));


            Factory = new MemoryRepositoryFactory();
            Factory.FromToken = token => RemoteRepositories.FirstOrDefault(repo => repo.Token == token);
            RemoteFactories = new List<IRemoteRepositoryFactory> { Factory };

            ServiceLocator.TryRegisterService<IEnumerable<IRemoteRepositoryFactory>>(() => RemoteFactories);

            ConfigurationManager = ServiceLocator.GetService<IConfigurationManager>();
            ConfigurationManager.Save(ConfiguredRemotes);

            ServiceLocator.RegisterService<IRemoteManager>(new DefaultRemoteManager(ConfigurationManager, ServiceLocator.GetService<IEnumerable<IRemoteRepositoryFactory>>()));


        }

        protected virtual Func<IPackageManager> PackageManagerFactory()
        {
            return () => new DefaultPackageManager(
                                 ServiceLocator.GetService<IPackageDeployer>(),
                                 ServiceLocator.GetService<IPackageResolver>(),
                                 ServiceLocator.GetService<IPackageExporter>());
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
            given_project_repository();
            AddPackage(Environment.ProjectRepository, name, version, dependencies);
        }

        protected void given_project_repository(IPackageRepository repository = null)
        {
            if (Environment.ProjectRepository == null)
                Environment.ProjectRepository = repository ?? new InMemoryRepository("Project repository") { CanLock = true };
        }

        protected void given_current_directory_repository(CurrentDirectoryRepository repository)
        {
            Environment.CurrentDirectoryRepository = repository;
        }

        protected void given_remote_package(string name, Version version, params string[] dependencies)
        {
            // note Version is a version type because of overload resolution...
            AddPackage(RemoteRepositories.First(), name, version.ToString(), dependencies);
        }

        protected void given_remote_package(string repositoryName, string name, Version version, params string[] dependencies)
        {
            AddPackage(RemoteRepositories.First(x => x.Name == repositoryName), name, version.ToString(), dependencies);
        }

        protected void given_system_package(string name, string version, params string[] dependencies)
        {
            AddPackage(Environment.SystemRepository, name, version, dependencies);
        }

        static void AddPackage(IPackageRepository repository, string name, string version, string[] dependencies)
        {
            var packageFileName = name + "-" + version + ".wrap";
            var packageStream = Packager.NewWithDescriptor(new InMemoryFile(packageFileName), name, version.ToString(), dependencies).OpenRead();
            using (var readStream = packageStream)
            using (var publisher = repository.Feature<ISupportPublishing>().Publisher())
                publisher.Publish(packageFileName, readStream);
            repository.RefreshPackages();
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
        protected void given_remote_config(string name, string fetchToken = "[memory]default", int? priority = null, string fetchUsername = null, string fetchPassword = null, params string[] publishTokens)
        {
            publishTokens = publishTokens ?? new string[0];
            fetchToken = fetchToken == "[memory]default" ? "[memory]" + name : fetchToken;
            var remote = new RemoteRepository
            {
                Name = name,
                Priority = priority ?? ConfiguredRemotes.Count + 1,
                PublishRepositories = publishTokens.Select(x => new RemoteRepositoryEndpoint { Token = x }).ToList()
            };
            if (fetchToken != null)
                remote.FetchRepository = new RemoteRepositoryEndpoint
                {
                    Token = fetchToken,
                    Username = fetchUsername,
                    Password = fetchPassword
                };
            ConfiguredRemotes.Add(remote);
            ConfigurationManager.Save(ConfiguredRemotes);
            ReloadRepositories();
        }

        protected void ReloadRepositories()
        {
            ConfiguredRemotes = ConfigurationManager.Load<RemoteRepositories>();
        }
        protected void given_config<T>(T config)
        {
            var confMan = ServiceLocator.GetService<IConfigurationManager>();
            confMan.Save(config);
        }
        protected void given_remote_config(RemoteRepositories remoteRepositories)
        {
            ServiceLocator.GetService<IConfigurationManager>().Save(ConfiguredRemotes = remoteRepositories);
            ReloadRepositories();
        }

        protected void given_file(string filePath, Stream stream)
        {

            var file = FileSystem.GetFile(filePath);
            using (var newFile = file.OpenWrite())
            {
                stream.CopyTo(newFile);
            }
        }

        protected void given_remote_repository(string remoteName, int? priority = null, Action<InMemoryRepository> factory = null)
        {
            var repo = new InMemoryRepository(remoteName);
            if (factory != null) factory(repo);
            RemoteRepositories.Add(repo);
            ConfiguredRemotes[remoteName] = new RemoteRepository
            {
                Priority = priority ?? ConfiguredRemotes.Count + 1,
                FetchRepository = new RemoteRepositoryEndpoint{Token=repo.Token},
                PublishRepositories = { new RemoteRepositoryEndpoint{Token=repo.Token } },
                Name = remoteName
            };
            ServiceLocator.GetService<IConfigurationManager>().Save(ConfiguredRemotes);
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

        protected void given_remote_factory(Func<string, NetworkCredential, IPackageRepository> repoFactory = null, Func<string, IPackageRepository> tokenFactory = null)
        {
            if (repoFactory != null) Factory.FromUserInput = repoFactory;
            if (tokenFactory != null) Factory.FromToken = tokenFactory;
        }

        protected void given_remote_factory_memory(Action<InMemoryRepository> factory = null)
        {
            Func<string, InMemoryRepository> build = name =>
            {
                var repo = new InMemoryRepository(name);
                if (factory != null) factory(repo);
                return repo;
            };
            given_remote_factory((name,cred) => build(name), token => build(token.Substring("[memory]".Length)));
        }

        protected void given_remote_factory_additional(Func<string, NetworkCredential, IPackageRepository> fromUserInput = null, Func<string, IPackageRepository> fromToken = null)
        {
            RemoteFactories.Add(new MemoryRepositoryFactory
            {
                FromToken = fromToken ?? (input => null),
                FromUserInput = fromUserInput ?? ((input,cred) => null)
            });
        }

        protected void given_locked_package(string name, string version)
        {
            Environment.ProjectRepository.Feature<ISupportLocking>()
                .Lock(string.Empty,
                      Environment.ProjectRepository
                          .PackagesByName[name]
                          .Where(x => x.Version == version.ToVersion()));

        }
    }

    public abstract class command<T> : openwrap_context where T : ICommand
    {
        protected ICommandDescriptor Command;
        protected List<ICommandOutput> Results;
        protected T CommandInstance;

        public command()
        {
            Command = CecilCommandExporter.GetCommandFrom<T>();
            Commands = new CommandRepository { Command };

        }

        protected virtual void when_executing_command(string args = null)
        {
            Console.WriteLine("> {0}-{1} {2}", Command.Verb, Command.Noun, args);
            args = args ?? string.Empty;
            foreach (var descriptor in Environment.ScopedDescriptors.Values)
                descriptor.Save();

            var runner = new CommandLineRunner();
            Results = runner.Run(Command, args).ToList();
            CommandInstance = (T)runner.LastCommand;
            Results.ForEach(_ => _.ToString().Split(new[]{"\r\n", "\r", "\n"},StringSplitOptions.None).Select(line => "< " + line).ToList().ForEach(Console.WriteLine));
            ReloadRepositories();
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