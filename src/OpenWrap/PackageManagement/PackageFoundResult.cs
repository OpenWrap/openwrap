using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Commands;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;

namespace OpenWrap.PackageManagement
{
    public class PackageFoundResult : PackageOperationResult
    {
        public PackageFoundResult(IGrouping<string, IPackageInfo> packageInfos, PackageListOptions options)
        {
            Name = packageInfos.Key;
            Packages = packageInfos.ToList();
            HasDetail = options == PackageListOptions.Detailed;
            if (HasDetail)
                Description = Packages.Last().Description;
            HasDetail = !string.IsNullOrEmpty(Description);
        }

        public string Description { get; private set; }
        public bool HasDetail { get; private set; }
        public string Name { get; private set; }
        public List<IPackageInfo> Packages { get; set; }

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