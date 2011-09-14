using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using OpenFileSystem.IO;
using OpenWrap.ProjectModel;
using OpenWrap.VisualStudio.SolutionAddIn;

namespace OpenWrap.VisualStudio.ProjectModel
{
    public class DteSolution : ISolution, IDisposable
    {
        Solution _solution;
        List<DteProject> _projects;
        bool _disposed;
        SolutionEvents _solutionEvents;

        public event EventHandler<EventArgs> ProjectChanged = (src,ea)=> { };

        public DteSolution(Solution solution)
        {
            _solution = solution;
            _projects = _solution.Projects.OfType<Project>().Select(x => new DteProject(x)).ToList();
            _solutionEvents = _solution.DTE.Events.SolutionEvents;
            _solutionEvents.ProjectAdded += HandleProjectAdded;
            _solutionEvents.ProjectRemoved += HandleProjectRemoved;
            _solutionEvents.ProjectRenamed += HandleProjectRenamed;
            Version = new Version(solution.DTE.Version);
        }

        void HandleProjectRenamed(Project project, string oldname)
        {
            if (_disposed) return;
            ProjectChanged.Raise(this, EventArgs.Empty);
        }

        void HandleProjectRemoved(Project project)
        {
            if (_disposed) return;

            lock (_projects)
                _projects.RemoveAll(x => x.DteObject == project);
            ProjectChanged.Raise(this, EventArgs.Empty);

        }

        void HandleProjectAdded(Project project)
        {
            if (_disposed) return;

            lock(_projects)
                _projects.Add(new DteProject(project));
            ProjectChanged.Raise(this, EventArgs.Empty);
        }

        public IEnumerable<IProject> AllProjects
        {
            get
            {
                CheckAccess();
                lock (_projects)
                {
                    return _projects.Cast<IProject>().ToList();
                }
            }
        }

        public Version Version { get; private set; }

        public bool OpenWrapAddInEnabled
        {
            get
            {
                CheckAccess();
                _solution.AddIns.Update();
                return _solution.AddIns.OfType<AddIn>().Any(x => x.ProgID == ComConstants.ADD_IN_PROGID_2010 || x.ProgID == ComConstants.ADD_IN_PROGID_2008);
            }
            set
            {
                CheckAccess();
                if (value && !OpenWrapAddInEnabled)
                {
                    if (_solution.DTE.Version == "9.0")
                        _solution.AddIns.Add(ComConstants.ADD_IN_PROGID_2008, ComConstants.ADD_IN_DESCRIPTION, ComConstants.ADD_IN_NAME, true);
                    else if (_solution.DTE.Version == "10.0")
                        _solution.AddIns.Add(ComConstants.ADD_IN_PROGID_2010, ComConstants.ADD_IN_DESCRIPTION, ComConstants.ADD_IN_NAME, true);
                }
                else if (value == false && OpenWrapAddInEnabled)
                {
                    _solution.AddIns.Cast<AddIn>()
                        .Where(x => x.ProgID == ComConstants.ADD_IN_PROGID_2008 ||
                                    x.ProgID == ComConstants.ADD_IN_PROGID_2010)
                        .ToList().ForEach(x => x.Remove());
                }
            }
        }

        public void Save()
        {
            CheckAccess();
            if (!_solution.IsDirty) return;
            _solution.DTE.ExecuteCommand("File.SaveAll");
            while (_solution.IsDirty)
            {
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }

        public IProject AddProject(IFile projectFile)
        {
            CheckAccess();
            return new DteProject(_solution.AddFromFile(projectFile.Path));
        }
        void CheckAccess()
        {
            if (_disposed)
                throw new ObjectDisposedException("The DteSolution instance has been disposed.");
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _solutionEvents.ProjectAdded -= HandleProjectAdded;
            _solutionEvents.ProjectRemoved -= HandleProjectRemoved;
            _solutionEvents.ProjectRenamed -= HandleProjectRenamed;
            _solutionEvents = null;
            _projects = null;

            _solution = null;
        }
    }
}