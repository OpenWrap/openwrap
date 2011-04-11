namespace OpenWrap.VisualStudio.ComShim
{
    public class MSBuild3Project : IMSBuildProject
    {
        readonly Microsoft.Build.BuildEngine.Project _project;

        public MSBuild3Project(Microsoft.Build.BuildEngine.Project project)
        {
            _project = project;
        }
        public void RunTarget(string target)
        {
            if (_project.Targets != null && _project.Targets.Exists(target))
                _project.Build(target);
        }
    }
}