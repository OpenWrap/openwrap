using OpenWrap.Commands;
using OpenWrap.Repositories;

namespace OpenWrap.PackageManagement
{
    public class PackageMissingResult : PackageResolveError
    {
        public PackageMissingResult(ResolvedPackage result)
                : base(result)
        {
        }

        public override ICommandOutput ToOutput()
        {
            return new Error("Package {0} not found in repositories.", Package.Identifier);
        }
    }
}