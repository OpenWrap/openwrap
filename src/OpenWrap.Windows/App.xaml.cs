using System;
using System.Windows;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystem.Local;
using OpenWrap.Commands;
using OpenWrap.Configuration;
using OpenWrap.Repositories;
using OpenWrap.Resolvers;
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
            Services.Services.TryRegisterService<IConfigurationManager>(() => new ConfigurationManager(Services.Services.GetService<IFileSystem>().GetDirectory(InstallationPaths.ConfigurationDirectory)));
            Services.Services.TryRegisterService<IEnvironment>(() => new CurrentDirectoryEnvironment());

            Services.Services.TryRegisterService<IPackageResolver>(() => new PackageResolver());
            Services.Services.RegisterService<ITaskManager>(new TaskManager());

            var repo = new CommandRepository(Services.Services.GetService<IEnvironment>().Commands());

            Services.Services.TryRegisterService<ICommandRepository>(() => repo);
            base.OnStartup(e);
        }
    }
}
