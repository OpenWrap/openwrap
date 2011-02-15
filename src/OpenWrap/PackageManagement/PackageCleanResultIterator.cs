using System.Collections.Generic;

namespace OpenWrap.PackageManagement
{
    public class PackageCleanResultIterator : AbstractPackageOperationResultIterator, IPackageCleanResult
    {
        public PackageCleanResultIterator(IEnumerable<PackageOperationResult> root) : base(root)
        {
        }
    }
}