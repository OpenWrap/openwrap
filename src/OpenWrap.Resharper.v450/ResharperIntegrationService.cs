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

        public ResharperIntegrationService(ExecutionEnvironment environment)
        {
            this.Environment = environment;
        }

        protected ExecutionEnvironment Environment { get; set; }

        public void Initialize()
        {
        }

        public void TryAddNotifier(IFile descriptorPath, IPackageRepository repository, string projectFilePath, IEnumerable<string> ignoredAssemblies)
        {
            //Debugger.Launch();
            if (_projectFiles.ContainsKey(projectFilePath))
            {
                WrapServices.GetService<IWrapDescriptorMonitoringService>().ProcessWrapDescriptor(descriptorPath, repository, _projectFiles[projectFilePath]);
                return;
            }
            _projectFiles[projectFilePath] = new ResharperProjectUpdater(descriptorPath, repository, projectFilePath, Environment, ignoredAssemblies);
        }
    }
}