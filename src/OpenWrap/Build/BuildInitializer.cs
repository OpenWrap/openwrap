// ReSharper disable UnusedMember.Global
using System;
using System.Collections.Generic;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.Local;
using OpenWrap.IO;
using OpenWrap.PackageModel;
using OpenWrap.Runtime;
using OpenWrap.Services;

namespace OpenWrap.Build
{
    public static class BuildInitializer
    {
        public static IDictionary<string, string> Initialize(string projectFile, string currentDirectory)
        {

            var cd = LocalFileSystem.Instance.GetDirectory(currentDirectory);
            new ServiceRegistry()
                .Override<IEnvironment>(() => new CurrentDirectoryEnvironment(cd))
                .Initialize();

            var env = ServiceLocator.GetService<IEnvironment>();

            var scope = PathFinder.GetCurrentScope(env.Descriptor.DirectoryStructure, new Path(projectFile));

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