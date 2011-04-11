namespace OpenWrap.VisualStudio.ComShim
{
    public class MSBuild3Engine : IMSBuildEngine
    {
        public IMSBuildProject Load(string fullPath)
        {
            return new MSBuild3Project(Microsoft.Build.BuildEngine.Engine.GlobalEngine.GetLoadedProject(fullPath));
        }
    }
}