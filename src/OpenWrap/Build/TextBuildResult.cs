namespace OpenWrap.Build
{
    public class TextBuildResult : BuildResult
    {
        public TextBuildResult(string text)
        {
            Message = text;
        }
    }
}