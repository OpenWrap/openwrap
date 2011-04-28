using System;
using System.Linq;
using System.Windows;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.Local;
using OpenWrap.Commands;
using OpenWrap.Configuration;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.AssemblyResolvers;
using OpenWrap.PackageManagement.DependencyResolvers;
using OpenWrap.PackageManagement.Deployers;
using OpenWrap.PackageManagement.Exporters;
using OpenWrap.PackageManagement.Exporters.Assemblies;
using OpenWrap.Preloading;
using OpenWrap.Runtime;
using OpenWrap.Services;
using OpenWrap.Tasks;
using OpenWrap.Windows.Framework.Messaging;
using OpenWrap.Windows.MainWindow;

namespace OpenWrap.Windows
{
    /// <summary>
    ///   Interaction logic for applicatiiion
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Preloader.LoadAssemblies(
                    Preloader.GetPackageFolders(Preloader.RemoteInstall.None, DefaultInstallationPaths.SystemRepositoryDirectory, "openfilesystem", "sharpziplib")
                    );
            RegisterServices();
            ShowMainWindow();
            SendInitalDataPopulationMessage();
            base.OnStartup(e);
        }

        static void RegisterServices()
        {
            ServiceLocator.RegisterService(new RuntimeAssemblyResolver());
            ServiceLocator.TryRegisterService<IFileSystem>(() => LocalFileSystem.Instance);
            ServiceLocator.TryRegisterService<IConfigurationManager>(
                    () => new DefaultConfigurationManager(ServiceLocator.GetService<IFileSystem>().GetDirectory(DefaultInstallationPaths.ConfigurationDirectory)));
            ServiceLocator.TryRegisterService<IEnvironment>(() => new CurrentDirectoryEnvironment());

            ServiceLocator.TryRegisterService<IPackageResolver>(() => new ExhaustiveResolver());
            ServiceLocator.TryRegisterService<IPackageExporter>(() => new DefaultPackageExporter(
                                                                              new IExportProvider[]
                                                                              {
                                                                                      new DefaultAssemblyExporter()
                                                                              }));
            ServiceLocator.TryRegisterService<IPackageDeployer>(() => new DefaultPackageDeployer());
            ServiceLocator.TryRegisterService<IPackageManager>(() => new DefaultPackageManager(
                                                                             ServiceLocator.GetService<IPackageDeployer>(),
                                                                             ServiceLocator.GetService<IPackageResolver>(),
                                                                             ServiceLocator.GetService<IPackageExporter>()));

            ServiceLocator.RegisterService<ITaskManager>(new TaskManager());
            var commands = Services.ServiceLocator.GetService<IPackageManager>().CommandExports(ServiceLocator.GetService<IEnvironment>());

            var commandRepository = new CommandRepository(commands.SelectMany(x => x).Select(x => x.Descriptor));

            ServiceLocator.TryRegisterService<ICommandRepository>(() => commandRepository);
        }

        static void SendInitalDataPopulationMessage()
        {
            Messenger.Default.Send(MessageNames.NounsVerbsChanged);
            Messenger.Default.Send(MessageNames.RepositoryListChanged);
        }

        void ShowMainWindow()
        {
            MainViewModel viewModel = new MainViewModel();

            Main mainWindow = new Main();
            mainWindow.DataContext = viewModel;

            MainWindow = mainWindow;
            mainWindow.Show();
        }
    }
}