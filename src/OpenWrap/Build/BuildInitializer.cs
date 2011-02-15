// ReSharper disable UnusedMember.Global
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.Local;
using OpenWrap.Configuration;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.AssemblyResolvers;
using OpenWrap.PackageManagement.DependencyResolvers;
using OpenWrap.PackageManagement.Deployers;
using OpenWrap.PackageManagement.Exporters;
using OpenWrap.Runtime;
using OpenWrap.Tasks;

namespace OpenWrap.Build
{

    public static class BuildInitializer
    {
        public static string Initialize(string projectFile, string currentDirectory)
        {   
            Services.ServiceLocator.TryRegisterService<IFileSystem>(() => LocalFileSystem.Instance);
            Services.ServiceLocator.TryRegisterService<IConfigurationManager>(
                    () => new DefaultConfigurationManager(Services.ServiceLocator.GetService<IFileSystem>().GetDirectory(DefaultInstallationPaths.ConfigurationDirectory)));
            Services.ServiceLocator.TryRegisterService<IEnvironment>(() => new MSBuildEnvironment(System.IO.Path.GetDirectoryName(projectFile), currentDirectory));

            Services.ServiceLocator.TryRegisterService<IPackageResolver>(() => new ExhaustiveResolver());
            Services.ServiceLocator.TryRegisterService<IPackageDeployer>(() => new DefaultPackageDeployer());
            Services.ServiceLocator.TryRegisterService<IPackageExporter>(() => new DefaultPackageExporter());
            Services.ServiceLocator.TryRegisterService(()=>new RuntimeAssemblyResolver());
            Services.ServiceLocator.TryRegisterService<ITaskManager>(()=>new TaskManager());

            Services.ServiceLocator.GetService<RuntimeAssemblyResolver>().Initialize();

            return Services.ServiceLocator.GetService<IEnvironment>().Descriptor.Name;
        }
    }
}
// ReSharper restore UnusedMember.Global