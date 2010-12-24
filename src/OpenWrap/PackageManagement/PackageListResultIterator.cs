using System.Collections.Generic;

namespace OpenWrap.PackageManagement
{
    public class PackageListResultIterator : AbstractPackageOperation, IPackageListResult
    {
        public PackageListResultIterator(IEnumerable<PackageOperationResult> root) : base(root)
        {
        }
    }
}