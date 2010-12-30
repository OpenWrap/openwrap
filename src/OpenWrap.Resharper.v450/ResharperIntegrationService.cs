extern alias resharper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using OpenFileSystem.IO;
using OpenWrap.PackageManagement.Monitoring;
using OpenWrap.Repositories;
using OpenWrap.Runtime;
using OpenWrap.Services;
using JetBrainsKey = resharper::JetBrains.Util.Key;


namespace OpenWrap.Resharper
{
    public class ResharperIntegrationService : IService
    {
        const int RETRY_DELAY_MS = 1000;
        const string MSBUILD_NS = "http://schemas.microsoft.com/developer/msbuild/2003";
        readonly object _lock = new object();
        readonly IPackageDescriptorMonitor _monitor;
        bool _isInitialized;
// ReSharper disable UnaccessedField.Local
        Timer _timer;
// ReSharper restore UnaccessedField.Local

        public ResharperIntegrationService()
                : this(Services.Services.GetService<IPackageDescriptorMonitor>())
        {
        }

        public ResharperIntegrationService(IPackageDescriptorMonitor monitor)
        {
            _monitor = monitor;
        }

        public void BootstrapSolution(Func<ExecutionEnvironment> environment, IFile descriptorPath, IPackageRepository repository)
        {
            if (!_isInitialized)
            {
                lock (_lock)
                {
                    if (_isInitialized)
                        return;


                    IEnumerable<IResolvedAssembliesUpdateListener> listeners = null;
                    bool success = false;
                    try
                    {
                        ResharperLocks.WriteCookie("Listing projects",
                                                   () => { success = TryEnumerateProjects(environment, out listeners); });
                        listeners = listeners ?? Enumerable.Empty<IResolvedAssembliesUpdateListener>();
                    }
                    catch (Exception e)
                    {
                        ResharperLogger.Debug("Exception when updating resharper: \r\n" + e);
                        success = false;
                    }
                    if (success)
                    {
                        foreach (var project in listeners)
                            _monitor.RegisterListener(descriptorPath, repository, project);
                        _isInitialized = true;
                    }
                    else
                    {
                        ResharperLogger.Debug("Import failed, rescheduling in one second.");
                        _timer = new Timer(x => BootstrapSolution(environment, descriptorPath, repository), null, RETRY_DELAY_MS, Timeout.Infinite);
                    }
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

        static bool TryEnumerateProjects(Func<ExecutionEnvironment> env, out IEnumerable<IResolvedAssembliesUpdateListener> projects)
        {
            projects = new IResolvedAssembliesUpdateListener[0];

            if (resharper::JetBrains.ProjectModel.SolutionManager.Instance == null)
                return false;

            ResharperLogger.Debug("SolutionManager found");

            resharper::JetBrains.ProjectModel.ISolution solution = resharper::JetBrains.ProjectModel.SolutionManager.Instance.CurrentSolution;
            if (solution == null) return false;

            ResharperLogger.Debug("Solution found");
            var monitors = (from proj in solution.GetAllProjects()
                            where ProjectIsOpenWrapEnabled(proj)
                            select new ResharperProjectUpdater(proj, env))
                    .Cast<IResolvedAssembliesUpdateListener>()
                    .ToList();
            projects = monitors;
            return true;
        }
    }
}