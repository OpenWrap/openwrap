namespace OpenWrap.Build.BuildEngines
{
    public class TextBuildResult : BuildResult
    {
        public TextBuildResult(string text)
        {
            Text = text;
        }

        public string Text { get; set; }
    }
}