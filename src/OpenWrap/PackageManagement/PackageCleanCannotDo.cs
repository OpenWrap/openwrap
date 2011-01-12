using OpenWrap.Commands;
using OpenWrap.PackageModel;

namespace OpenWrap.PackageManagement
{
    public class PackageCleanCannotDo : PackageOperationResult
    {
        readonly IPackageDescriptor _projectDescriptor;

        public PackageCleanCannotDo(IPackageDescriptor projectDescriptor)
        {
            _projectDescriptor = projectDescriptor;
        }

        public override bool Success
        {
            get { return false; }
        }

        public override ICommandOutput ToOutput()
        {
            return new Error("Cannot clean package as it wasn't found.");
        }
    }
}