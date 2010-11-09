using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystem.Local;
using OpenWrap.Configuration;
using OpenWrap.Preloading;
using OpenWrap.Repositories;
using OpenWrap.Resolvers;
using OpenWrap.Services;
using OpenWrap.Tasks;

namespace OpenWrap.Build
{
    public class InitializeOpenWrap : Microsoft.Build.Utilities.Task
    {
        public string CurrentDirectory { get; set; }

        public InitializeOpenWrap()
        {
            Preloader.GetPackageFolders(Preloader.RemoteInstall.None, InstallationPaths.SystemRepositoryDirectory, "openfilesystem", "sharpziplib");
            
        }
        [Output]
        public string Name { get; set; }

        public bool StartDebug { get; set; }

        RuntimeAssemblyResolver _resolver;
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
            Services.Services.TryRegisterService<IConfigurationManager>(() => new ConfigurationManager(Services.Services.GetService<IFileSystem>().GetDirectory(InstallationPaths.ConfigurationDirectory)));
            Services.Services.TryRegisterService<IEnvironment>(() => new MSBuildEnvironment(task, currentDirectory));

            Services.Services.TryRegisterService<IPackageResolver>(() => new PackageResolver());
            Services.Services.RegisterService<RuntimeAssemblyResolver>(new RuntimeAssemblyResolver());
            Services.Services.RegisterService<ITaskManager>(new TaskManager());
        }
    }
}
