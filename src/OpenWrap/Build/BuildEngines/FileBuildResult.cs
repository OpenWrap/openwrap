using System;
using OpenFileSystem.IO;

namespace OpenWrap.Build.BuildEngines
{
    public class FileBuildResult : BuildResult, IEquatable<FileBuildResult>
    {
        // TODO: Move Path manipulation to the OFS Path class
        public FileBuildResult(string exportName, Path file)
        {
            if (exportName == null) throw new ArgumentNullException("exportName");
            if (file == null) throw new ArgumentNullException("file");
            ExportName = exportName;
            Path = file;
            FileName = System.IO.Path.GetFileName(Path.FullPath);
        }

        public string ExportName { get; private set; }
        public Path Path { get; private set; }
        public string FileName { get; private set; }

        public bool Equals(FileBuildResult other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.ExportName, ExportName) && Equals(other.FileName, FileName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(FileBuildResult)) return false;
            return Equals((FileBuildResult)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (ExportName.GetHashCode() * 397) ^ FileName.GetHashCode();
            }
        }
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