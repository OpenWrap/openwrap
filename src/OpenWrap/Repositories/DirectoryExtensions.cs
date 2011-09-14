using System.Linq;
using OpenFileSystem.IO;

namespace OpenWrap.Repositories
{
    public static class DirectoryExtensions
    {
        public static IDirectory FindProjectRepositoryDirectory(this IDirectory root)
        {
            return root == null ? null : root.GetDirectory("wraps");
        }
    }
}