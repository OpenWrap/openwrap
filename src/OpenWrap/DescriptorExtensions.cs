using System.Linq;
using OpenFileSystem.IO;

namespace OpenWrap
{
    public static class DescriptorExtensions
    {
        public static IDirectory FindProjectRepositoryDirectory(this IDirectory root)
        {
            return root == null
                           ? null
                           : root.AncestorsAndSelf()
                                     .SelectMany(x => x.Directories("wraps"))
                                     .Where(x => x != null)
                                     .FirstOrDefault();
        }
    }
}