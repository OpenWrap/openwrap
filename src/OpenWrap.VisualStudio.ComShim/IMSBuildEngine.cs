namespace OpenWrap.VisualStudio.ComShim
{
    public interface IMSBuildEngine
    {
        IMSBuildProject Load(string fullPath);
    }
}