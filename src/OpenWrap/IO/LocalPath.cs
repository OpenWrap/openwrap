using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenWrap.IO
{
    public class LocalPath : IPath
    {
        public LocalPath(string fullPath)
        {
            FullPath = fullPath;
            FileSystem = IO.FileSystem.Local;
            IsRooted = Path.IsPathRooted(fullPath);
            
            
                GenerateSegments();
            
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
    }
}