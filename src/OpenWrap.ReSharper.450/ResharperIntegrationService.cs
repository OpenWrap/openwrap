extern alias resharper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using OpenWrap.IO;
using OpenWrap.PackageManagement.Monitoring;
using OpenWrap.Repositories;
using OpenWrap.Runtime;
using OpenWrap.Services;
using JetBrainsKey = resharper::JetBrains.Util.Key;


namespace OpenWrap.Resharper
{
    public class ResharperIntegrationService : IService
    {
        const string MSBUILD_NS = "http://schemas.microsoft.com/developer/msbuild/2003";
        const int REFRESH_DELAY_MS = 500;
        const int RETRY_DELAY_MS = 1000;
        readonly object _lock = new object();
        readonly IPackageDescriptorMonitor _monitor;
        readonly IEnvironment _env;
        readonly Timer _refreshTimer;

        Func<ExecutionEnvironment> _environment;
// ReSharper disable UnaccessedField.Local
        Timer _initializeTimer;
// ReSharper restore UnaccessedField.Local
        bool _isInitialized;
        List<ResharperProjectUpdater> _knownProjects;
        IPackageRepository _projectRepository;

// ReSharper disable UnusedMember.Global
        public ResharperIntegrationService()
// ReSharper restore UnusedMember.Global
                : this(ServiceLocator.GetService<IPackageDescriptorMonitor>(), ServiceLocator.GetService<IEnvironment>())
        {
        }

// ReSharper disable MemberCanBePrivate.Global
        public ResharperIntegrationService(IPackageDescriptorMonitor monitor, IEnvironment env)
// ReSharper restore MemberCanBePrivate.Global
        {
            _monitor = monitor;
            _env = env;
            _refreshTimer = new Timer(_ => Refresh(), null, Timeout.Infinite, Timeout.Infinite);
        }

        static resharper::JetBrains.ProjectModel.ISolution Solution
        {
            get { return resharper::JetBrains.ProjectModel.SolutionManager.Instance.CurrentSolution; }
        }

        static resharper::JetBrains.ProjectModel.SolutionManager SolutionManager
        {
            get { return resharper::JetBrains.ProjectModel.SolutionManager.Instance; }
        }

// ReSharper disable UnusedMember.Global
        public void BootstrapSolution(Func<ExecutionEnvironment> environment, IPackageRepository projectRepository)
// ReSharper restore UnusedMember.Global
        {
            if (!_isInitialized)
            {
                lock (_lock)
                {
                    if (_isInitialized)
                        return;
                    _environment = environment;
                    _projectRepository = projectRepository;


                    IEnumerable<ResharperProjectUpdater> listeners = null;
                    bool success;
                    try
                    {
                        success = ListProjects(environment, out listeners);
                    }
                    catch (Exception e)
                    {
                        ResharperLogger.Debug("Exception when updating resharper: \r\n" + e);
                        success = false;
                    }

                    if (success)
                    {
                        RegisterChangeMonitor();
                        foreach (var project in listeners)
                            _monitor.RegisterListener(project.Descriptor, projectRepository, project);

                        _knownProjects = listeners.ToList();
                        _isInitialized = true;
                    }
                    else
                    {
                        ResharperLogger.Debug("Import failed, rescheduling in one second.");
                        _initializeTimer = new Timer(x => BootstrapSolution(environment, projectRepository), null, RETRY_DELAY_MS, Timeout.Infinite);
                    }
                }
            }
        }

        void Refresh()
        {
            lock (_lock)
            {
                ResharperLogger.Debug("Refreshing list of projects.");

                IEnumerable<ResharperProjectUpdater> tempCurrentProjects;
                ListProjects(_environment, out tempCurrentProjects);
                var currentProjects = tempCurrentProjects.ToList();
                
                foreach (var project in currentProjects.ToList())
                {
                    var existing = currentProjects.FirstOrDefault(x => string.Equals(x.ProjectPath, project.ProjectPath, StringComparison.OrdinalIgnoreCase));
                    if (existing != null)
                    {
                        currentProjects.Remove(project);
                        continue;
                    }
                    _monitor.RegisterListener(project.Descriptor, _projectRepository, project);
                    _knownProjects.Add(project);
                }
                foreach (var oldProject in currentProjects)
                {
                    _monitor.UnregisterListener(oldProject);
                }
            }
        }

        public void Initialize()
        {
        }

        static bool ProjectIsOpenWrapEnabled(resharper::JetBrains.ProjectModel.IProject proj)
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

        bool HasProjectChanges(resharper::JetBrains.ProjectModel.SolutionChange solutionChanges)
        {
            var children = solutionChanges.GetChildren();
            foreach (var child in children.OfType<resharper::JetBrains.ProjectModel.ProjectItemChange>())
            {
                if (child.IsAdded || child.IsRemoved ||child.IsExternallyChanged || (child.IsSubtreeChanged && HasProjectItemChanges(child)))
                    return true;
            }
            return false;
        }

        bool HasProjectItemChanges(resharper::JetBrains.ProjectModel.ProjectItemChange child)
        {
            var children = child.GetChildren();
            return children.OfType<resharper::JetBrains.ProjectModel.AssemblyChange>().Any();
        }

        bool HasSolutionChanges(resharper::JetBrains.ProjectModel.SolutionChange solutionChanges)
        {
            return solutionChanges.IsAdded ||
                   solutionChanges.IsRemoved ||
                   solutionChanges.IsOpeningSolution ||
                   solutionChanges.IsClosingSolution;
        }

        bool ListProjects(Func<ExecutionEnvironment> environment, out IEnumerable<ResharperProjectUpdater> projects)
        {
            bool success = false;
            IEnumerable<ResharperProjectUpdater> readProjects = null;
            //Guard.Run("Listing projects",
            //                           () => { success = TryEnumerateProjects(environment, out readProjects); });
            projects = readProjects ?? Enumerable.Empty<ResharperProjectUpdater>();
            return success;
        }

        void RegisterChangeMonitor()
        {
            resharper::JetBrains.Application.ChangeManager.Instance.Changed += HandleChanges;
        }

        void ScheduleRefresh()
        {
            _refreshTimer.Change(REFRESH_DELAY_MS, Timeout.Infinite);
        }


        void HandleChanges(object sender, resharper::JetBrains.Application.ChangeEventArgs changeeventargs)
        {
            var solutionChanges = changeeventargs.ChangeMap.GetChange(Solution) as resharper::JetBrains.ProjectModel.SolutionChange;
            if (solutionChanges == null)
            {
                ResharperLogger.Debug("Unknown solution change");
                return;
            }
            if (HasSolutionChanges(solutionChanges) ||
                HasProjectChanges(solutionChanges))
            {
                ResharperLogger.Debug("Scheduled refresh of projects");
                ScheduleRefresh();
            }
        }
    }
}