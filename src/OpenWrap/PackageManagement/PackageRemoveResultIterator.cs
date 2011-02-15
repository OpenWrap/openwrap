using System.Collections.Generic;

namespace OpenWrap.PackageManagement
{
    public class PackageRemoveResultIterator : AbstractPackageOperationResultIterator, IPackageRemoveResult
    {
        public PackageRemoveResultIterator(IEnumerable<PackageOperationResult> root) : base(root)
        {
        }
    }
}