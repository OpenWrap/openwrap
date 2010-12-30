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
using OpenWrap.Runtime;
using OpenWrap.Tasks;
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
            RegisterServices();
            ShowMainWindow();
            base.OnStartup(e);
        }

        private static void RegisterServices()
        {
            Services.Services.RegisterService(new RuntimeAssemblyResolver());
            Services.Services.TryRegisterService<IFileSystem>(() => LocalFileSystem.Instance);
            Services.Services.TryRegisterService<IConfigurationManager>(() => new DefaultConfigurationManager(Services.Services.GetService<IFileSystem>().GetDirectory(DefaultInstallationPaths.ConfigurationDirectory)));
            Services.Services.TryRegisterService<IEnvironment>(() => new CurrentDirectoryEnvironment());

            Services.Services.TryRegisterService<IPackageResolver>(() => new ExhaustiveResolver());
            Services.Services.TryRegisterService<IPackageExporter>(() => new DefaultPackageExporter());
            Services.Services.TryRegisterService<IPackageDeployer>(() => new DefaultPackageDeployer());
            Services.Services.TryRegisterService<IPackageManager>(() => new DefaultPackageManager(
                Services.Services.GetService<IPackageDeployer>(),
                Services.Services.GetService<IPackageResolver>()));

            Services.Services.RegisterService<ITaskManager>(new TaskManager());

            var repo = new CommandRepository(Services.Services.GetService<IEnvironment>().Commands());

            Services.Services.TryRegisterService<ICommandRepository>(() => repo);
        }

        private void ShowMainWindow()
        {
            MainViewModel viewModel = new MainViewModel();
            viewModel.PopulateData();

            Main mainWindow = new Main();
            mainWindow.DataContext = viewModel;

            MainWindow = mainWindow;
            mainWindow.Show();
        }
    }
}
