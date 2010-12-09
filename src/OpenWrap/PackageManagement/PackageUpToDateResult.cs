using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Commands;
using OpenWrap.Repositories;

namespace OpenWrap.PackageManagement
{
    internal class PackageUpToDateResult : PackageOperationResult
    {
        readonly IPackageInfo _existingUpToDateVersion;
        readonly ISupportPublishing _repository;

        public PackageUpToDateResult(IPackageInfo existingUpToDateVersion, ISupportPublishing repository)
        {
            _existingUpToDateVersion = existingUpToDateVersion;
            _repository = repository;
        }

        public override bool Success
        {
            get { return true; }
        }

        public override ICommandOutput ToOutput()
        {
            return new Info("{0}: {1} up-to-date.", _repository.Name, _existingUpToDateVersion.Identifier.Name);
        }
    }
    public class PackageFoundResult : PackageOperationResult
    {
        public PackageFoundResult(IGrouping<string, IPackageInfo> packageInfos)
        {
            Name = packageInfos.Key;
            Packages = packageInfos.ToList();
        }

        public List<IPackageInfo> Packages { get; set; }

        public string Name { get; private set; }

        public override bool Success
        {
            get { return true; }
        }

        public override ICommandOutput ToOutput()
        {
            return new PackageFoundCommandOutput(this);
        }
    }
    public class PackageFoundCommandOutput : GenericMessage
    {
        readonly PackageFoundResult _result;

        public PackageFoundCommandOutput(PackageFoundResult result)
            : base(" - {0} (available: {1})", result.Name, result.Packages.Select(x => x.Version + (x.Nuked ? " [nuked]" : string.Empty)).Join(", "))
        {
            
            _result = result;
        }
        public string Name { get { return _result.Name; } }

    }
}
