namespace OpenWrap.Build
{
    public class TextBuildResult : BuildResult
    {
        public TextBuildResult(string text)
        {
            Message = text;
        }
    }
    public class InfoBuildResult : BuildResult
    {
        public InfoBuildResult(string text)
        {
            Message = text;
        }
    }
}