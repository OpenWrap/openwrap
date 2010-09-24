using OpenFileSystem.IO.FileSystem.Local;

namespace OpenWrap.Build.BuildEngines
{
    public class FileBuildResult : BuildResult
    {
        public FileBuildResult(string exportName, LocalPath file)
        {
            ExportName = exportName;
            Path = file;
        }

        public string ExportName { get; private set; }
        public LocalPath Path { get; private set; }
    }
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