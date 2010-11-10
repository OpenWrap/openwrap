using System;
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
            {
                if (_projectFiles.ContainsKey(projectFilePath))
                {
                    ResharperLogger.Debug("TryAddNotifier: updating {0}.", projectFilePath);
                    Services.Services.GetService<IWrapDescriptorMonitoringService>().ProcessWrapDescriptor(descriptorPath, repository, _projectFiles[projectFilePath]);
                    return;
                }
                ResharperLogger.Debug("TryAddNotifier: adding {0} as new project.", projectFilePath);
                _projectFiles[projectFilePath] = new ResharperProjectUpdater(descriptorPath, repository, projectFilePath, environment, ignoredAssemblies);
            }

        }
    }

    static class ResharperLogger
    {
        public static void Debug(string text, params string[] args)
        {
            Debugger.Log(0, "resharper", DateTime.Now.ToShortTimeString() + ":" + string.Format(text, args) + "\r\n");

        }
    }
}