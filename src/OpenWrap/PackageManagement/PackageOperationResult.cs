using OpenWrap.Commands;
using OpenWrap.Repositories;

namespace OpenWrap.PackageManagement
{
    public abstract class PackageOperationResult
    {
        public abstract bool Success { get; }
        
        public abstract ICommandOutput ToOutput();
    }
}