using System.IO;
using System.Linq;

namespace OpenWrap.IO
{
    public class LocalPath : IPath
    {
        public LocalPath(string fullPath)
        {
            FullPath = fullPath;
        }

        public IFileSystem FileSystem
        {
            get { return IO.FileSystem.Local; }
        }

        public string FullPath { get; private set; }

        public IPath Combine(params string[] paths)
        {
            var combinedPath = paths.Aggregate(FullPath, Path.Combine);
            return new LocalPath(combinedPath);
        }
    }
}