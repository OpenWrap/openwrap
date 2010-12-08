using System;
using OpenWrap.Commands;
using OpenWrap.PackageManagement;

namespace OpenWrap.Repositories
{
    public class PackageCleanResult : PackageOperationResult
    {
        bool _success;

        public PackageCleanResult(IPackageInfo package, bool success)
        {
            Package = package;
            _success = success;
        }

        public IPackageInfo Package { get; private set; }
        public override bool Success { get { return _success; } }
        public override ICommandOutput ToOutput()
        {
            if (Success) return new Info("Package {0} removed.", Package.Identifier);
            return new Error("Package {0} could not be removed. If Visual Studio, notepad or a folder is still open, please close them and try the command again.", Package.Identifier);
        }
    }
}