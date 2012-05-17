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
        {
            Repository = repository;
            Packages = packages;
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(Repository.Name);
            
            if (Packages.Any() == false) return sb.ToString();

            var currentPackage = Packages.First();
            foreach(var package in Packages.Skip(1))
            {
                sb.AppendFormat("├─{0}{1} [{2}]\r\n", currentPackage.Name, GenerateTitle(currentPackage), GenerateVersions(currentPackage));
                currentPackage = package;
            }
            sb.AppendFormat("└─{0}{1} [{2}]\r\n", currentPackage.Name, GenerateTitle(currentPackage), GenerateVersions(currentPackage));
            return sb.ToString();
        }

        object GenerateTitle(PackageFoundResult currentPackage)
        {
            var title = currentPackage.Packages.First().Title;
            return title != null ? string.Format(" ({0})") : string.Empty;
        }

        string GenerateVersions(PackageFoundResult package)
        {
            Func<IPackageInfo, SemanticVersion> versionSelector = x => 
                Detailed ? x.SemanticVersion : x.SemanticVersion.Numeric();
            return package.Packages.Select(versionSelector)
                                   .Distinct()
                                   .OrderByDescending(_=>_)
                                   .JoinString(", ");
        }
    }
}