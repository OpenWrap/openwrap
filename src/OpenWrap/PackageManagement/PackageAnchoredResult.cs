using System;
using OpenWrap.Commands;
using OpenWrap.Repositories;

namespace OpenWrap.PackageManagement
{
    public class PackageAnchoredResult : PackageOperationResult
    {
        readonly ISupportAnchoring _repository;
        readonly IPackageInfo _package;
        
        bool _success;

        public PackageAnchoredResult(ISupportAnchoring repository, IPackageInfo package, bool success)
        {
            _repository = repository;
            _package = package;
            _success = success;
        }

        public override bool Success
        {
            get { return true; }
        }

        public override ICommandOutput ToOutput()
        {
            if (_success == false)
                return new Warning("{0}: Package {1} could not be anchored.", _repository.Name, _package.Identifier);
            
                return new Info("{0}: Package {1} anchored.", _repository.Name, _package.Identifier);
        }
    }
}