using System.Collections.Generic;
using OpenRasta.Wrap.Build.Services;
using OpenRasta.Wrap.Dependencies;
using OpenRasta.Wrap.Sources;

namespace OpenWrap.Resharper
{
    public class ResharperIntegrationService : IService
    {
        Dictionary<string, ResharperProjectUpdater> _projectFiles = new Dictionary<string, ResharperProjectUpdater>();

        public ResharperIntegrationService(WrapRuntimeEnvironment environment)
        {
            this.Environment = environment;
        }

        protected WrapRuntimeEnvironment Environment { get; set; }

        public void Initialize()
        {
        }

        public void TryAddNotifier(string descriptorPath, IWrapRepository repository, string projectFilePath)
        {
            if (_projectFiles.ContainsKey(projectFilePath))
                return;
            _projectFiles[projectFilePath] = new ResharperProjectUpdater(descriptorPath, repository, projectFilePath, Environment);
        }
    }
}