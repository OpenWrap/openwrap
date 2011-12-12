using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.Commands;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;
using OpenWrap;

namespace OpenWrap.PackageManagement
{
    public class RepositoryPackages : Info
    {
        public IPackageRepository Repository { get; set; }
        public IEnumerable<PackageFoundResult> Packages { get; private set; }
        public bool Detailed { get; set; }

        public RepositoryPackages(IPackageRepository repository, IEnumerable<PackageFoundResult> packages)
//                : base(" - {0} (available: {1})", result.Name, result.Packages.Select(x => x.Version + (x.Nuked ? " [nuked]" : string.Empty)).JoinString(", "))
        {
            Repository = repository;
            Packages = packages;
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(Repository.Name);

            foreach(var package in Packages)
                sb.AppendFormat("└─{0} ({1})\r\n", package.Name, GenerateVersions(package));
            return sb.ToString();
        }

        string GenerateVersions(PackageFoundResult package)
        {
            Func<IPackageInfo, SemanticVersion> versionSelector = x => 
                Detailed ? x.Version : x.Version.Numeric();
            return package.Packages.Select(versionSelector)
                                   .Distinct()
                                   .OrderByDescending(_=>_)
                                   .JoinString(", ");
        }
    }
}