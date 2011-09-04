extern alias resharper;
using ResharperPluginManager = resharper::JetBrains.UI.Application.PluginSupport.PluginManager;
using ResharperPlugin = resharper::JetBrains.UI.Application.PluginSupport.Plugin;
using ResharperPluginTitleAttribute = resharper::JetBrains.UI.Application.PluginSupport.PluginTitleAttribute;
using ResharperPluginDescriptionAttribute = resharper::JetBrains.UI.Application.PluginSupport.PluginDescriptionAttribute;
using ResharperSolutionComponentAttribute = resharper::JetBrains.ProjectModel.SolutionComponentImplementationAttribute;

namespace OpenWrap.Resharper
{
    [ResharperSolutionComponent]
    public class LegacyProjectReferenceUpdater : resharper::JetBrains.ProjectModel.ISolutionComponent
    {
        readonly resharper::JetBrains.ProjectModel.ISolution _solution;
        ProjectReferenceUpdater _updater;

        public LegacyProjectReferenceUpdater(resharper::JetBrains.ProjectModel.ISolution solution)
        {
            _solution = solution;
        }

        public void Dispose()
        {
            _updater.Dispose();
        }

        public void Init()
        {
        }

        public void AfterSolutionOpened()
        {
            _updater = new ProjectReferenceUpdater(_solution, resharper::JetBrains.Application.ChangeManager.Instance, new LegacyShellThreading());
        }

        public void BeforeSolutionClosed()
        {
            _updater.Dispose();
        }
    }
}