using System.Collections.Generic;
using System.Linq;
using OpenWrap.Commands;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;

namespace OpenWrap.PackageManagement
{
    public class PackageFoundResult : PackageOperationResult
    {
        public PackageFoundResult(IGrouping<string, IPackageInfo> packageInfos, PackageListOptions options, IEnumerable<IPackageInfo> currentPackages)
        {
            Options = options;
            CurrentPackages = currentPackages;
            Name = packageInfos.Key;
            Packages = packageInfos.ToList();
        }

        public string Name { get; private set; }
        public List<IPackageInfo> Packages { get; set; }
        public PackageListOptions Options { get; private set; }
        public IEnumerable<IPackageInfo> CurrentPackages { get; set; }

        public override bool Success
        {
            get { return true; }
        }

        public override ICommandOutput ToOutput()
        {
            return new PackageFoundCommandOutput(this);
        }
    }
}