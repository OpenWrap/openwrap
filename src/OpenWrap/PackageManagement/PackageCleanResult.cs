using OpenWrap.Commands;
using OpenWrap.PackageModel;

namespace OpenWrap.PackageManagement
{
    public class PackageCleanResult : PackageOperationResult
    {
        readonly bool _success;

        public PackageCleanResult(IPackageInfo package, bool success)
        {
            Package = package;
            _success = success;
        }

        public IPackageInfo Package { get; private set; }

        public override bool Success
        {
            get { return _success; }
        }

        public override ICommandOutput ToOutput()
        {
            if (Success) return new Info("Package file {0} removed.", Package.Identifier);
            return new Warning("Package file {0} could not be removed. If Visual Studio, notepad or a folder window is still open, it could could be holding a lock on a file. Please close it and try the command again.", Package.Identifier);
        }
    }
}