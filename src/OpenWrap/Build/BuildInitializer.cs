// ReSharper disable UnusedMember.Global
using System;
using System.Collections.Generic;
using System.IO;
using OpenWrap.IO;
using OpenWrap.PackageManagement.AssemblyResolvers;
using OpenWrap.PackageModel;
using OpenWrap.Runtime;
using OpenWrap.Services;

namespace OpenWrap.Build
{
    public static class BuildInitializer
    {
        public static IDictionary<string, string> Initialize(string projectFile, string currentDirectory)
        {
            new ServiceRegistry()
                .Override<IEnvironment>(() => new MSBuildEnvironment(Path.GetDirectoryName(projectFile), currentDirectory))
                .Initialize();

            var env = ServiceLocator.GetService<IEnvironment>();

            var scope = PathFinder.GetCurrentScope(env.Descriptor.DirectoryStructure, new OpenFileSystem.IO.Path(projectFile));

            var currentDescriptor = env.GetOrCreateScopedDescriptor(scope);
            return new Dictionary<string, string>
            {
                { BuildConstants.PACKAGE_NAME, currentDescriptor.Value.Name },
                { BuildConstants.PROJECT_SCOPE, scope },
                { BuildConstants.DESCRIPTOR_PATH, currentDescriptor.File.Path.FullPath }
            };
        }
    }
}

// ReSharper restore UnusedMember.Global