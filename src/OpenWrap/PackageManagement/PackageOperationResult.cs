using OpenWrap.Commands;

namespace OpenWrap.PackageManagement
{
    public abstract class PackageOperationResult
    {
        public abstract ICommandOutput ToOutput();
    }
}