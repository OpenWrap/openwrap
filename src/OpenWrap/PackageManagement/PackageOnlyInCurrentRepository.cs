using OpenWrap.Commands;
using OpenWrap.Repositories;

namespace OpenWrap.PackageManagement
{
    public class PackageOnlyInCurrentRepository : PackageOperationResult, ICommandOutput
    {
        public string PackageName { get; set; }
        public IPackageRepository PackageRepository { get; set; }

        public PackageOnlyInCurrentRepository(string packageName, IPackageRepository packageRepository)
        {
            PackageName = packageName;
            PackageRepository = packageRepository;
            Type = CommandResultType.Warning;
        }

        public override bool Success
        {
            get { return true; }
        }

        public override ICommandOutput ToOutput()
        {
            return this;
        }

        public CommandResultType Type { get; set; }

        public override string ToString()
        {
            return string.Format("Package '{0}' is only found in repository '{1}', are you missing a remote?", PackageName, PackageRepository.Name);
        }
    }
}