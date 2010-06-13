using System.Collections.Generic;
using OpenWrap.Build.Services;
using OpenWrap.Dependencies;
using OpenWrap.IO;
using OpenWrap.Repositories;

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

        public void TryAddNotifier(IFile descriptorPath, IPackageRepository repository, string projectFilePath)
        {
            if (_projectFiles.ContainsKey(projectFilePath))
                return;
            _projectFiles[projectFilePath] = new ResharperProjectUpdater(descriptorPath, repository, projectFilePath, Environment);
        }
    }
}