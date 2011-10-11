using System;
using OpenWrap.Commands;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;

namespace OpenWrap.PackageManagement
{
    public class PackageUpToDateResult : PackageOperationResult, ICommandOutput
    {
        public IPackageInfo Package { get; private set; }
        public IPackageRepository DestinationRepository { get; private set; }

        public PackageUpToDateResult(IPackageInfo existingUpToDateVersion, IPackageRepository repository)
        {
            Package = existingUpToDateVersion;
            DestinationRepository = repository;
        }

        public override bool Success
        {
            get { return true; }
        }

        public override ICommandOutput ToOutput()
        {
            return this;
        }

        public CommandResultType Type
        {
            get { return CommandResultType.Info;}
        }
        public override string ToString()
        {
            return string.Format("{0}: {1} up-to-date.", DestinationRepository.Name, Package.Identifier.Name);
        }
    }
}