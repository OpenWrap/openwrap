using System;
using OpenFileSystem.IO;

namespace OpenWrap.Build
{
    public class FileBuildResult : BuildResult, IEquatable<FileBuildResult>
    {
        // TODO: Move Path manipulation to the OFS Path class
        public FileBuildResult(string exportName, Path file, bool allowBinDuplicate)
        {
            if (exportName == null) throw new ArgumentNullException("exportName");
            if (file == null) throw new ArgumentNullException("file");
            ExportName = exportName;
            Path = file;
            FileName = System.IO.Path.GetFileName(Path.FullPath);
            AllowBinDuplicate = allowBinDuplicate;
        }

        public string ExportName { get; private set; }
        public string FileName { get; private set; }
        public Path Path { get; private set; }
        public bool AllowBinDuplicate { get; private set; }

        public bool Equals(FileBuildResult other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.ExportName, ExportName) && Equals(other.FileName, FileName) && other.AllowBinDuplicate.Equals(AllowBinDuplicate);
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
                int result = (ExportName != null ? ExportName.GetHashCode() : 0);
                result = (result * 397) ^ (FileName != null ? FileName.GetHashCode() : 0);
                result = (result * 397) ^ AllowBinDuplicate.GetHashCode();
                return result;
            }
        }
    }
}