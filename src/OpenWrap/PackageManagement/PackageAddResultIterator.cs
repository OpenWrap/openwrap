using System.Collections.Generic;

namespace OpenWrap.PackageManagement
{
    public class PackageAddResultIterator : AbstractPackageOperationResultIterator, IPackageAddResult
    {
        public PackageAddResultIterator(IEnumerable<PackageOperationResult> root) : base(root)
        {
        }
    }
}