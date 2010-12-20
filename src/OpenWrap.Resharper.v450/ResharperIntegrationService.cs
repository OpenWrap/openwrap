extern alias resharper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;

using OpenWrap.Build;
using OpenWrap.Dependencies;
using OpenFileSystem.IO;
using OpenWrap.Repositories;
using OpenWrap.Services;
using JetBrainsKey = resharper::JetBrains.Util.Key;


namespace OpenWrap.Resharper
{
    public class ResharperIntegrationService : IService
    {
        readonly IWrapDescriptorMonitoringService _monitor;
        Dictionary<string, ResharperProjectUpdater> _projectFiles = new Dictionary<string, ResharperProjectUpdater>();
        bool _isInitialized = false;
        object _lock = new object();
        Timer _timer;
        public const int RETRY_DELAY_MS = 1000;

        public ResharperIntegrationService()
            : this(Services.Services.GetService<IWrapDescriptorMonitoringService>())
        {

        }

        public ResharperIntegrationService(IWrapDescriptorMonitoringService monitor)
        {
            _monitor = monitor;
        }

        public void Initialize()
        {
        }

        public void BootstrapSolution(Func<ExecutionEnvironment> environment, IFile descriptorPath, IPackageRepository repository)
        {
            if (!_isInitialized)
            {
                lock(_lock)
                {
                    if (_isInitialized)
                        return;


                    IEnumerable<IPackageAssembliesListener> listeners = null;
                    bool success = false;
                    try
                    {
                        ResharperLocks.WriteCookie("Listing projects",
                                                   () =>
                                                   {
                                                       success = TryEnumerateProjects(environment, out listeners);
                                                   });
                    }
                    catch(Exception e)
                    {
                        ResharperLogger.Debug("Exception when updating resharper: \r\n" + e);
                        success = false;
                    }
                    if (success)
                    {
                        foreach (var project in listeners)
                            _monitor.ProcessWrapDescriptor(descriptorPath, repository, project);
                    }
                    else
                    {
                        ResharperLogger.Debug("Import failed, rescheduling in one second.");
                        _timer = new Timer(x => BootstrapSolution(environment, descriptorPath, repository), null, RETRY_DELAY_MS, Timeout.Infinite);

                    }
                }
            }
        }

        bool TryEnumerateProjects(Func<ExecutionEnvironment> env, out IEnumerable<IPackageAssembliesListener> projects)
        {
            projects = new IPackageAssembliesListener[0];

            if (resharper::JetBrains.ProjectModel.SolutionManager.Instance == null)
                return false;

            ResharperLogger.Debug("SolutionManager found");

            resharper::JetBrains.ProjectModel.ISolution solution = resharper::JetBrains.ProjectModel.SolutionManager.Instance.CurrentSolution;
            if (solution == null) return false;

            ResharperLogger.Debug("Solution found");
            var monitors = (from proj in solution.GetAllProjects()
                            where ProjectIsOpenWrapEnabled(proj)
                            select new ResharperProjectUpdater(proj, env))
                            .Cast<IPackageAssembliesListener>()
                            .ToList();
            projects = monitors;
            return true;
        }
        const string MSBUILD_NS = "http://schemas.microsoft.com/developer/msbuild/2003";

        bool ProjectIsOpenWrapEnabled(resharper::JetBrains.ProjectModel.IProject proj)
        {
            if (proj.ProjectFile == null || !File.Exists(proj.ProjectFile.Location.FullPath))
                return false;
            var xmlDoc = new XmlDocument();
            var namespaceManager = new XmlNamespaceManager(xmlDoc.NameTable);
            namespaceManager.AddNamespace("msbuild", MSBUILD_NS);

            using (var projectFileStream = File.OpenRead(proj.ProjectFile.Location.FullPath))
                xmlDoc.Load(projectFileStream);
            var isOpenWrap = (from node in xmlDoc.SelectNodes(@"//msbuild:Import", namespaceManager).OfType<XmlElement>()
                                let attr = node.GetAttribute("Project")
                                where attr != null && Regex.IsMatch(attr, @"OpenWrap\..*\.targets")
                                select node).Any();
            var isDisabled =
                    (
                            from node in xmlDoc.SelectNodes(@"//msbuild:OpenWrap-EnableVisualStudioIntegration", namespaceManager).OfType<XmlElement>()
                            let value = node.Value
                            where value != null && value.EqualsNoCase("false")
                            select node
                    ).Any();
            return isOpenWrap && !isDisabled;
        }
    }
}