using System.Collections.Generic;

namespace OpenWrap.PackageManagement
{
    public interface IPackageUpdateResult : IEnumerable<PackageOperationResult>
    {
    }
}