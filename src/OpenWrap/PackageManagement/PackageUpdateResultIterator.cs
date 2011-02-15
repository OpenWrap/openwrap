using System.Collections.Generic;

namespace OpenWrap.PackageManagement
{
    public class PackageUpdateResultIterator : AbstractPackageOperationResultIterator, IPackageUpdateResult
    {
        public PackageUpdateResultIterator(IEnumerable<PackageOperationResult> root) : base(root)
        {
        }
    }
}