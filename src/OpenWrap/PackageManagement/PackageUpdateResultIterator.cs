using System.Collections.Generic;

namespace OpenWrap.PackageManagement
{
    public class PackageUpdateResultIterator : AbstractPackageOperation, IPackageUpdateResult
    {
        public PackageUpdateResultIterator(IEnumerable<PackageOperationResult> root) : base(root)
        {
        }
    }
}