using System;
using OpenWrap.Windows.Framework;

namespace OpenWrap.Windows.AllPackages
{
    public class PackageViewModel : ViewModelBase
    {
        public string Name { get; set; }
        public Version LatestVersion { get; set; }
        public string Description { get; set; }
        public string Source { get; set; }

        public DateTimeOffset Created { get; set; }
        public bool IsInstalledSystem { get; set; }
        public bool IsInstalledInDirectory { get; set; }

        public string CreatedDisplay
        {
            get { return Created.ToString("d") + " " + Created.ToString("t"); }
        }
    }
}
