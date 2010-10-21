using System.Collections.Generic;
using System.Diagnostics;
using OpenWrap.Dependencies;
using OpenFileSystem.IO;
using OpenWrap.Repositories;
using OpenWrap.Services;

namespace OpenWrap.Resharper
{
    public class ResharperIntegrationService : IService
    {
        Dictionary<string, ResharperProjectUpdater> _projectFiles = new Dictionary<string, ResharperProjectUpdater>();

        public ResharperIntegrationService()
        {
        }

        public void Initialize()
        {
        }

        public void TryAddNotifier(ExecutionEnvironment environment, IFile descriptorPath, IPackageRepository repository, string projectFilePath, IEnumerable<string> ignoredAssemblies)
        {
            lock (_projectFiles)

                if (_projectFiles.ContainsKey(projectFilePath))
                {
                    Services.Services.GetService<IWrapDescriptorMonitoringService>().ProcessWrapDescriptor(descriptorPath, repository, _projectFiles[projectFilePath]);
                    return;
                }
            _projectFiles[projectFilePath] = new ResharperProjectUpdater(descriptorPath, repository, projectFilePath, environment, ignoredAssemblies);

        }
    }
}