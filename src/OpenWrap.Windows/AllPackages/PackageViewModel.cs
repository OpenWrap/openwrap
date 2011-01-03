using System;
using OpenWrap.Windows.Framework;

namespace OpenWrap.Windows.AllPackages
{
    public class PackageViewModel : ViewModelBase
    {
        public string Name { get; set; }
        public Version LatestVersion { get; set; }
        public string Description { get; set; }

        public DateTimeOffset Created { get; set; }
    }
}
