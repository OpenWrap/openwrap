using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystem.Local;
using OpenWrap.Configuration;
using OpenWrap.Repositories;
using OpenWrap.Resolvers;
using OpenWrap.Services;
using OpenWrap.Tasks;

namespace OpenWrap.Build
{
    public class InitializeOpenWrap : Microsoft.Build.Utilities.Task
    {
        static void IntializeOpenWrap()
        {
            Preloader.PreloadOpenWrapDependencies();
        }
        public string CurrentDirectory { get; set; }

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
            Name = WrapServices.GetService<IEnvironment>().Descriptor.Name;
            return true;
        }

        static void RegisterServices(InitializeOpenWrap task, string currentDirectory)
        {
            WrapServices.TryRegisterService<IFileSystem>(() => LocalFileSystem.Instance);
            WrapServices.TryRegisterService<IConfigurationManager>(() => new ConfigurationManager(WrapServices.GetService<IFileSystem>().GetDirectory(InstallationPaths.ConfigurationDirectory)));
            WrapServices.TryRegisterService<IEnvironment>(() => new MSBuildEnvironment(task, currentDirectory));

            WrapServices.TryRegisterService<IPackageManager>(() => new PackageManager());
            WrapServices.RegisterService<RuntimeAssemblyResolver>(new RuntimeAssemblyResolver());
            WrapServices.RegisterService<ITaskManager>(new TaskManager());
        }
    }

    public class MSBuildEnvironment : CurrentDirectoryEnvironment
    {
        public MSBuildEnvironment(InitializeOpenWrap initializeOpenWrap, string currentDirectory) : base(Path.GetDirectoryName(initializeOpenWrap.BuildEngine.ProjectFileOfTaskNode))
        {
            if (currentDirectory != null)
                CurrentDirectory = LocalFileSystem.Instance.GetDirectory(currentDirectory);
        }
    }
}
