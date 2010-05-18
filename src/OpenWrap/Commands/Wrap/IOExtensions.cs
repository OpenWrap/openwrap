using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenWrap.Commands.Wrap
{
    public static class IOExtensions
    {
        public static IEnumerable<DirectoryInfo> SelfAndAncestors(this DirectoryInfo di)
        {
            do
            {
                yield return di;
                di = di.Parent;
            } while (di != null);
        }
        public static IEnumerable<string> Files(this DirectoryInfo di, string filePattern)
        {
            return System.IO.Directory.GetFiles(di.FullName, filePattern, SearchOption.TopDirectoryOnly);
        }
        public static string File(this DirectoryInfo di, string filePattern)
        {
            return di.Files(filePattern).FirstOrDefault();
        }
        public static DirectoryInfo Directory(this DirectoryInfo di, string directoryName)
        {
            return di.Directories(directoryName).FirstOrDefault();
        }
        public static IEnumerable<DirectoryInfo> Directories(this DirectoryInfo di, string directoryName)
        {
            return System.IO.Directory.GetDirectories(di.FullName, directoryName, SearchOption.TopDirectoryOnly)
                .Select(x => new DirectoryInfo(x));
        }
    }
}