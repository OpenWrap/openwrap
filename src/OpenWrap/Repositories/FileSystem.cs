using System.IO;
using System.Linq;

namespace OpenWrap.Repositories
{
    public static class FileSystem
    {
        public static string CombinePaths(params string[] paths)
        {
            if (paths == null || paths.Length < 1) return null;
            
            var path = paths[0];
            foreach (var pathToCombine in paths.Skip(1))
                path = Path.Combine(path, pathToCombine);
            return path;
        }
    }
}