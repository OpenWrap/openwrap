using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.Local;
using OpenWrap.Build;
using OpenWrap.Commands;
using OpenWrap.Configuration;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.AssemblyResolvers;
using OpenWrap.PackageManagement.DependencyResolvers;
using OpenWrap.PackageManagement.Deployers;
using OpenWrap.PackageManagement.Exporters;
using OpenWrap.Repositories;
using OpenWrap.Runtime;
using OpenWrap.Services;
using OpenWrap.Tasks;

namespace OpenWrap.Windows
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            //Preloader.PreloadDependencies("openfilesystem", "sharpziplib");
        }
        protected override void OnStartup(StartupEventArgs e)
        {

            Services.Services.RegisterService<RuntimeAssemblyResolver>(new RuntimeAssemblyResolver());
            Services.Services.TryRegisterService<IFileSystem>(() => LocalFileSystem.Instance);
            Services.Services.TryRegisterService<IConfigurationManager>(() => new DefaultConfigurationManager(Services.Services.GetService<IFileSystem>().GetDirectory(DefaultInstallationPaths.ConfigurationDirectory)));
            Services.Services.TryRegisterService<IEnvironment>(() => new CurrentDirectoryEnvironment());


            Services.Services.TryRegisterService<IPackageResolver>(() => new ExhaustiveResolver());
            Services.Services.TryRegisterService<IPackageExporter>(() => new DefaultPackageExporter());
            Services.Services.TryRegisterService<IPackageDeployer>(() => new DefaultPackageDeployer());
            Services.Services.TryRegisterService<IPackageManager>(() => new DefaultPackageManager(
                Services.Services.GetService<IPackageDeployer>(),
                Services.Services.GetService<IPackageResolver>()
                ));


            Services.Services.RegisterService<ITaskManager>(new TaskManager());

            var repo = new CommandRepository(Services.Services.GetService<IEnvironment>().Commands());

            Services.Services.TryRegisterService<ICommandRepository>(() => repo);
            base.OnStartup(e);
        }
    }
}
