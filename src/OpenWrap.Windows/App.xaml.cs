using System;
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
using OpenWrap.Preloading;
using OpenWrap.Runtime;
using OpenWrap.Tasks;
using OpenWrap.Windows.Framework.Messaging;
using OpenWrap.Windows.MainWindow;

namespace OpenWrap.Windows
{
    /// <summary>
    /// Interaction logic for applicatiiion
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

        private static void RegisterServices()
        {
            Services.ServiceLocator.RegisterService(new RuntimeAssemblyResolver());
            Services.ServiceLocator.TryRegisterService<IFileSystem>(() => LocalFileSystem.Instance);
            Services.ServiceLocator.TryRegisterService<IConfigurationManager>(() => new DefaultConfigurationManager(Services.ServiceLocator.GetService<IFileSystem>().GetDirectory(DefaultInstallationPaths.ConfigurationDirectory)));
            Services.ServiceLocator.TryRegisterService<IEnvironment>(() => new CurrentDirectoryEnvironment());

            Services.ServiceLocator.TryRegisterService<IPackageResolver>(() => new ExhaustiveResolver());
            Services.ServiceLocator.TryRegisterService<IPackageExporter>(() => new DefaultPackageExporter());
            Services.ServiceLocator.TryRegisterService<IPackageDeployer>(() => new DefaultPackageDeployer());
            Services.ServiceLocator.TryRegisterService<IPackageManager>(() => new DefaultPackageManager(
                Services.ServiceLocator.GetService<IPackageDeployer>(),
                Services.ServiceLocator.GetService<IPackageResolver>()));

            Services.ServiceLocator.RegisterService<ITaskManager>(new TaskManager());

            var repo = new CommandRepository(Services.ServiceLocator.GetService<IEnvironment>().Commands());

            Services.ServiceLocator.TryRegisterService<ICommandRepository>(() => repo);
        }

        private void ShowMainWindow()
        {
            MainViewModel viewModel = new MainViewModel();

            Main mainWindow = new Main();
            mainWindow.DataContext = viewModel;

            MainWindow = mainWindow;
            mainWindow.Show();
        }

        private static void SendInitalDataPopulationMessage()
        {
            Messenger.Default.Send(MessageNames.NounsVerbsChanged);
            Messenger.Default.Send(MessageNames.RepositoryListChanged);
        }
    }
}
