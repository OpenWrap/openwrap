using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenWrap.IO.FileSystem.Local
{
    public class LocalPath : IPath, IEquatable<IPath>
    {
        string _normalizedPath;

        public LocalPath(string fullPath)
        {
            FullPath = fullPath;
            FileSystem = IO.FileSystems.Local;
            IsRooted = Path.IsPathRooted(fullPath);
            
            
                GenerateSegments();
            _normalizedPath = NormalizePath(fullPath);

        }

        public bool IsRooted { get; private set; }

        void GenerateSegments()
        {
            var segments = new List<string>();
            var di = new DirectoryInfo(FullPath);
            do
            {
                segments.Add(di.Name);
                di = di.Parent;
            } while (di != null);
            segments.Reverse();
            Segments = segments.AsReadOnly();
        }

        public IFileSystem FileSystem
        {
            get; set;
        }

        public string FullPath { get; private set; }
        
        public IPath Combine(params string[] paths)
        {
            var combinedPath = paths.Aggregate(FullPath, Path.Combine);
            return new LocalPath(combinedPath);
        }

        public IEnumerable<string> Segments
        {
            get; private set;
        }

        public bool Equals(IPath other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return NormalizePath(other.FullPath).Equals(_normalizedPath, StringComparison.OrdinalIgnoreCase);
        }

        string NormalizePath(string fullPath)
        {
            return fullPath.EndsWith(Path.DirectorySeparatorChar.ToString()) || fullPath.EndsWith(Path.AltDirectorySeparatorChar.ToString())
                ? fullPath.Substring(0, fullPath.Length - 1)
                : fullPath;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (!(obj is IPath)) return false;
            return Equals((IPath)obj);
        }

        public override int GetHashCode()
        {
            return (_normalizedPath != null ? _normalizedPath.GetHashCode() : 0);
        }

        public static bool operator ==(LocalPath left, LocalPath right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(LocalPath left, LocalPath right)
        {
            return !Equals(left, right);
        }
    }
}