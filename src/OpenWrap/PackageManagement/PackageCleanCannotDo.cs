using OpenWrap.Commands;
using OpenWrap.Dependencies;

namespace OpenWrap.PackageManagement
{
    public class PackageCleanCannotDo : PackageOperationResult
    {
        readonly PackageDescriptor _projectDescriptor;

        public PackageCleanCannotDo(PackageDescriptor projectDescriptor)
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