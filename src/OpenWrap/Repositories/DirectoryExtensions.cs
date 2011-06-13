using System.Linq;
using OpenFileSystem.IO;

namespace OpenWrap.Repositories
{
    public static class DirectoryExtensions
    {
        public static IDirectory FindProjectRepositoryDirectory(this IDirectory root)
        {
            // TODO: Review if we should not *only* take the wraps folder where the .wrapdesc is
            return root == null
                           ? null
                           : root.AncestorsAndSelf()
                                     .SelectMany(x => x.Directories("wraps"))
                                     .Where(x => x != null)
                                     .FirstOrDefault();
        }
    }
}