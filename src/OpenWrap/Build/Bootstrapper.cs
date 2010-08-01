using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystem.Local;
using OpenWrap.Configuration;
using OpenWrap.Repositories;
using OpenWrap.Resolvers;
using OpenWrap.Services;
using OpenWrap.Tasks;
using Task = Microsoft.Build.Utilities.Task;

namespace OpenWrap.Build
{
    public class Bootstrapper : Task
    {
        RuntimeAssemblyResolver _resolver;
        public override bool Execute()
        {
            WrapServices.TryRegisterService<IFileSystem>(() => LocalFileSystem.Instance);
            WrapServices.TryRegisterService<IConfigurationManager>(() => new ConfigurationManager(WrapServices.GetService<IFileSystem>().GetDirectory(InstallationPaths.ConfigurationDirectory)));
            WrapServices.TryRegisterService<IEnvironment>(() => new MSBuildEnvironment(this));

            WrapServices.TryRegisterService<IPackageManager>(() => new PackageManager());
            WrapServices.RegisterService<RuntimeAssemblyResolver>(new RuntimeAssemblyResolver());
            WrapServices.RegisterService<ITaskManager>(new TaskManager());
            _resolver = new RuntimeAssemblyResolver();
            _resolver.Initialize();
            return true;
        }
    }

    public class MSBuildEnvironment : CurrentDirectoryEnvironment
    {
        public MSBuildEnvironment(Bootstrapper bootstrapper) : base(Path.GetDirectoryName(bootstrapper.BuildEngine.ProjectFileOfTaskNode))
        {
            
        }
    }
}
