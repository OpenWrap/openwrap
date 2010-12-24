using System.Collections.Generic;

namespace OpenWrap.PackageManagement
{
    public class PackageAddResultIterator : AbstractPackageOperation, IPackageAddResult
    {
        public PackageAddResultIterator(IEnumerable<PackageOperationResult> root) : base(root)
        {
        }
    }
}