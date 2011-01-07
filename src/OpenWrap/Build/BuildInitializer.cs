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
            Services.Services.TryRegisterService<IFileSystem>(() => LocalFileSystem.Instance);
            Services.Services.TryRegisterService<IConfigurationManager>(
                    () => new DefaultConfigurationManager(Services.Services.GetService<IFileSystem>().GetDirectory(DefaultInstallationPaths.ConfigurationDirectory)));
            Services.Services.TryRegisterService<IEnvironment>(() => new MSBuildEnvironment(System.IO.Path.GetDirectoryName(projectFile), currentDirectory));

            Services.Services.TryRegisterService<IPackageResolver>(() => new ExhaustiveResolver());
            Services.Services.TryRegisterService<IPackageDeployer>(() => new DefaultPackageDeployer());
            Services.Services.TryRegisterService<IPackageExporter>(() => new DefaultPackageExporter());
            Services.Services.TryRegisterService(()=>new RuntimeAssemblyResolver());
            Services.Services.TryRegisterService<ITaskManager>(()=>new TaskManager());

            Services.Services.GetService<RuntimeAssemblyResolver>().Initialize();

            return Services.Services.GetService<IEnvironment>().Descriptor.Name;
        }
    }
}
// ReSharper restore UnusedMember.Global