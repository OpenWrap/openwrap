using System.Collections.Generic;

namespace OpenWrap.PackageManagement
{
    public class PackageListResultIterator : AbstractPackageOperationResultIterator, IPackageListResult
    {
        public PackageListResultIterator(IEnumerable<PackageOperationResult> root) : base(root)
        {
        }
    }
}