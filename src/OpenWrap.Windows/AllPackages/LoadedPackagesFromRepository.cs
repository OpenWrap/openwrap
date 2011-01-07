using System.Collections.Generic;
using OpenWrap.Repositories;

namespace OpenWrap.Windows.AllPackages
{
    public class LoadedPackagesFromRepository
    {
        public IPackageRepository Repository { get; set; }
        public IList<PackageViewModel> Packages { get; set; }
    }
}