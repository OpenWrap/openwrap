namespace OpenWrap.Build.BuildEngines
{
    public class TextBuildResult : BuildResult
    {
        public TextBuildResult(string text)
        {
            Message = text;
        }
    }
}