using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Build.Framework;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.Local;
using OpenWrap.Configuration;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.AssemblyResolvers;
using OpenWrap.PackageManagement.DependencyResolvers;
using OpenWrap.PackageManagement.Deployers;
using OpenWrap.PackageManagement.Exporters;
using OpenWrap.Preloading;
using OpenWrap.Repositories;
using OpenWrap.Runtime;
using OpenWrap.Tasks;
using Task = Microsoft.Build.Utilities.Task;

namespace OpenWrap.Build
{
    public class InitializeOpenWrap : Task
    {
        RuntimeAssemblyResolver _resolver;

        public InitializeOpenWrap()
        {
            var loadedAssemblies = Preloader.LoadAssemblies(
                    Preloader.GetPackageFolders(Preloader.RemoteInstall.None, DefaultInstallationPaths.SystemRepositoryDirectory, "openfilesystem", "sharpziplib")
                    );
            if (!loadedAssemblies.Any(x => x.Value.EndsWithNoCase("openfilesystem.dll")))
                throw new InvalidOperationException("OpenFileSystem not found, dragons ahead.");
        }

        public string CurrentDirectory { get; set; }

        [Output]
        public string Name { get; set; }

        public bool StartDebug { get; set; }

        public override bool Execute()
        {
            if (StartDebug)
                Debugger.Launch();
            RegisterServices(this, CurrentDirectory);
            _resolver = new RuntimeAssemblyResolver();
            _resolver.Initialize();
            Name = Services.Services.GetService<IEnvironment>().Descriptor.Name;
            return true;
        }

        static void RegisterServices(InitializeOpenWrap task, string currentDirectory)
        {
            Services.Services.TryRegisterService<IFileSystem>(() => LocalFileSystem.Instance);
            Services.Services.TryRegisterService<IConfigurationManager>(
                    () => new DefaultConfigurationManager(Services.Services.GetService<IFileSystem>().GetDirectory(DefaultInstallationPaths.ConfigurationDirectory)));
            Services.Services.TryRegisterService<IEnvironment>(() => new MSBuildEnvironment(task, currentDirectory));

            Services.Services.TryRegisterService<IPackageResolver>(() => new ExhaustiveResolver());
            Services.Services.TryRegisterService<IPackageDeployer>(() => new DefaultPackageDeployer());
            Services.Services.TryRegisterService<IPackageExporter>(() => new DefaultPackageExporter());
            Services.Services.RegisterService(new RuntimeAssemblyResolver());
            Services.Services.RegisterService<ITaskManager>(new TaskManager());
        }
    }
}