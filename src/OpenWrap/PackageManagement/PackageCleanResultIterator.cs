using System.Collections.Generic;

namespace OpenWrap.PackageManagement
{
    public class PackageCleanResultIterator : AbstractPackageOperation, IPackageCleanResult
    {
        public PackageCleanResultIterator(IEnumerable<PackageOperationResult> root) : base(root)
        {
        }
    }
}