using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Mono.Cecil;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.Local;
using OpenRasta.Client;
using OpenWrap.Commands;
using OpenWrap.Commands.Cli;
using OpenWrap.Commands.Cli.Locators;
using OpenWrap.Configuration;
using OpenWrap.Configuration.Core;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.AssemblyResolvers;
using OpenWrap.PackageManagement.DependencyResolvers;
using OpenWrap.PackageManagement.Deployers;
using OpenWrap.PackageManagement.Exporters;
using OpenWrap.PackageManagement.Exporters.Assemblies;
using OpenWrap.PackageManagement.Exporters.Commands;
using OpenWrap.PackageManagement.Monitoring;
using OpenWrap.Repositories;
using OpenWrap.Repositories.FileSystem;
using OpenWrap.Repositories.Http;
using OpenWrap.Repositories.NuFeed;
using OpenWrap.Runtime;
using OpenWrap.Tasks;

namespace OpenWrap.Services
{
    public class ServiceRegistry
    {
        readonly Dictionary<Type, Action> _services = new Dictionary<Type, Action>();
        string _configDirectory = DefaultInstallationPaths.ConfigurationDirectory;
        string _currentDirectory = Environment.CurrentDirectory;
        string _systemRepositoryPath = DefaultInstallationPaths.SystemRepositoryDirectory;

        public ServiceRegistry()
        {
            Register<IFileSystem>(() => LocalFileSystem.Instance);

            Register<IConfigurationManager>(
                () => new DefaultConfigurationManager(
                          Get<IFileSystem>().GetDirectory(_configDirectory)));
            Register<IHttpClient>(() => new WebRequestHttpClient
            {
                Proxy = ReadProxy
            });
            Register<IEnumerable<IRemoteRepositoryFactory>>(() => new List<IRemoteRepositoryFactory>
            {
                new IndexedFolderRepositoryFactory(Get<IFileSystem>()),
                new NuFeedRepositoryFactory(
                    Get<IFileSystem>(),
                    Get<IHttpClient>()),
                new IndexedHttpRepositoryFactory(Get<IHttpClient>())
            });
            Register<IRemoteManager>(() => new DefaultRemoteManager(
                                               Get<IConfigurationManager>(),
                                               Get<IEnumerable<IRemoteRepositoryFactory>>()));

            Register<IEnvironment>(() => new CurrentDirectoryEnvironment(LocalFileSystem.Instance.GetDirectory(_currentDirectory))
            {
                SystemRepositoryDirectory = Get<IFileSystem>().GetDirectory(_systemRepositoryPath)
            });

            Register<IPackageResolver>(() => new StrategyResolver());
            Register<IPackageExporter>(() => new DefaultPackageExporter(new List<IExportProvider>
            {
                new DefaultAssemblyExporter(),
                new CecilCommandExporter(),
                new SolutionPluginExporter()
            }));
            Register<IEventHub>(() => new EventHub());

            Register<IPackageDeployer>(() => new DefaultPackageDeployer());
            Register<IPackageManager>(() => new DefaultPackageManager(
                                                Get<IPackageDeployer>(),
                                                Get<IPackageResolver>(),
                                                Get<IPackageExporter>()
                                                ));

            Register<ITaskManager>(() => new TaskManager());
            Register<IAssemblyResolver>(() => new CecilStaticAssemblyResolver(Get<IPackageManager>(), Get<IEnvironment>()));

            Register<IEnumerable<ICommandLocator>>(() => new ICommandLocator[]
            {
                new NounVerbCommandLocator(Get<ICommandRepository>()),
                new VerbNounCommandLocator(Get<ICommandRepository>()),
                new DefaultVerbCommandLocator(Get<ICommandRepository>())
            });

            Register<ICommandRepository>(() => new CommandRepository(
                                                   Get<IPackageManager>().CommandExports(Get<IEnvironment>())
                                                       .SelectMany(x => x)
                                                       .Select(x => x.Descriptor)));

            Register(() => new RuntimeAssemblyResolver());
            Register<IPackageDescriptorMonitor>(() => new PackageDescriptorMonitor());
            Register<ICommandOutputFormatter>(() => new ConsoleCommandOutputFormatter());
        }

        public ServiceRegistry ConfigurationDirectory(string path)
        {
            _configDirectory = path;
            return this;
        }

        public ServiceRegistry CurrentDirectory(string currentDirectory)
        {
            _currentDirectory = currentDirectory;
            return this;
        }

        public void Initialize()
        {
            ServiceLocator.Clear();
            foreach (var reg in _services.Values) reg();
            ServiceLocator.GetService<RuntimeAssemblyResolver>();
        }

        public ServiceRegistry Override<T>(Func<T> factory) where T : class
        {
            Register(factory);
            return this;
        }

        public ServiceRegistry SystemRepositoryDirectory(string systemRepositoryPath)
        {
            _systemRepositoryPath = systemRepositoryPath;
            return this;
        }

        static T Get<T>() where T : class
        {
            return ServiceLocator.GetService<T>();
        }

        static WebProxy ReadProxy()
        {
            var conf = Get<IConfigurationManager>().Load<CoreConfiguration>();
            if (conf == null || conf.ProxyHref == null) return null;
            var proxy = new WebProxy(conf.ProxyHref);
            if (conf.ProxyUsername == null)
                proxy.Credentials = new NetworkCredential(conf.ProxyUsername, conf.ProxyPassword);
            return proxy;
        }

        void Register<T>(Func<T> registration) where T : class
        {
            _services[typeof(T)] = () => ServiceLocator.TryRegisterService(registration);
        }
    }
}