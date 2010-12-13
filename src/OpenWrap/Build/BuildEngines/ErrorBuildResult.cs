namespace OpenWrap.Build.BuildEngines
{
    public class ErrorBuildResult : BuildResult
    {
        public ErrorBuildResult()
        {
        }

        public ErrorBuildResult(string message)
        {
            Message = message;
        }
    }
}