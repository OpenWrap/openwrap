using System.Collections.Generic;

namespace OpenWrap.PackageManagement
{
    public class PackageRemoveResultIterator : AbstractPackageOperation, IPackageRemoveResult
    {
        public PackageRemoveResultIterator(IEnumerable<PackageOperationResult> root) : base(root)
        {
        }
    }
}