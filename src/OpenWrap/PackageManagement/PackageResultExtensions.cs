using System.Collections.Generic;

namespace OpenWrap.PackageManagement
{
    public static class PackageResultExtensions
    {
        public static IEnumerable<PackageOperationResult> Hooks(this IEnumerable<PackageOperationResult> results)
        {
            bool success = true;
            foreach (var result in results)
            {
                if (result.Success == false)
                    success = false;
                yield return result;
            }
        }
    }
}