using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystem.Local;
using OpenWrap.Build;
using OpenWrap.Commands;
using OpenWrap.Configuration;
using OpenWrap.Repositories;
using OpenWrap.Resolvers;
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

            WrapServices.RegisterService<RuntimeAssemblyResolver>(new RuntimeAssemblyResolver());
            WrapServices.TryRegisterService<IFileSystem>(() => LocalFileSystem.Instance);
            WrapServices.TryRegisterService<IConfigurationManager>(() => new ConfigurationManager(WrapServices.GetService<IFileSystem>().GetDirectory(InstallationPaths.ConfigurationDirectory)));
            WrapServices.TryRegisterService<IEnvironment>(() => new CurrentDirectoryEnvironment());

            WrapServices.TryRegisterService<IPackageManager>(() => new PackageManager());
            WrapServices.RegisterService<ITaskManager>(new TaskManager());

            var repo = new CommandRepository(WrapServices.GetService<IEnvironment>().Commands());

            WrapServices.TryRegisterService<ICommandRepository>(() => repo);
            base.OnStartup(e);
        }
    }
}
