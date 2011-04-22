// ReSharper disable UnusedMember.Global
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.Local;
using OpenWrap.Commands;
using OpenWrap.Configuration;
using OpenWrap.IO;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.AssemblyResolvers;
using OpenWrap.PackageManagement.DependencyResolvers;
using OpenWrap.PackageManagement.Deployers;
using OpenWrap.PackageManagement.Exporters;
using OpenWrap.PackageManagement.Exporters.Assemblies;
using OpenWrap.PackageModel;
using OpenWrap.Runtime;
using OpenWrap.Services;
using OpenWrap.Tasks;

namespace OpenWrap.Build
{

    public static class BuildInitializer
    {
        public static IDictionary<string,string> Initialize(string projectFile, string currentDirectory)
        {   
            Services.ServiceLocator.TryRegisterService<IFileSystem>(() => LocalFileSystem.Instance);
            Services.ServiceLocator.TryRegisterService<IConfigurationManager>(
                    () => new DefaultConfigurationManager(Services.ServiceLocator.GetService<IFileSystem>().GetDirectory(DefaultInstallationPaths.ConfigurationDirectory)));
            Services.ServiceLocator.TryRegisterService<IEnvironment>(() => new MSBuildEnvironment(System.IO.Path.GetDirectoryName(projectFile), currentDirectory));

            Services.ServiceLocator.TryRegisterService<IPackageResolver>(() => new ExhaustiveResolver());
            Services.ServiceLocator.TryRegisterService<IPackageDeployer>(() => new DefaultPackageDeployer());
            Services.ServiceLocator.TryRegisterService<IPackageExporter>(() => new DefaultPackageExporter(new IExportProvider[]{
                    new EnvironmentDependentAssemblyExporter(ServiceLocator.GetService<IEnvironment>().ExecutionEnvironment)
                }));
            Services.ServiceLocator.TryRegisterService(()=>new RuntimeAssemblyResolver());
            Services.ServiceLocator.TryRegisterService<ITaskManager>(()=>new TaskManager());

            Services.ServiceLocator.GetService<RuntimeAssemblyResolver>().Initialize();
            var env = Services.ServiceLocator.GetService<IEnvironment>();
            var scope = PathFinder.GetCurrentScope(env.Descriptor.DirectoryStructure, new Path(projectFile));

            var currentDescriptor = env.GetOrCreateScopedDescriptor(scope);
            return new Dictionary<string, string>
            {
                    { BuildConstants.PACKAGE_NAME, currentDescriptor.Value.Name },
                    { BuildConstants.PROJECT_SCOPE, scope },
                    {BuildConstants.DESCRIPTOR_PATH, currentDescriptor.File.Path.FullPath}
            };
        }
    }
}
// ReSharper restore UnusedMember.Global