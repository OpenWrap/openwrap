using System;
using OpenWrap.Windows.Framework;

namespace OpenWrap.Windows.AllPackages
{
    public class PackageViewModel : ViewModelBase
    {
        private const int MaximumShortDescriptionLength = 40;

        public string Name { get; set; }
        public Version LatestVersion { get; set; }
        public int VersionCount { get; set; }

        public string Description { get; set; }
        public string DescriptionShort
        {
            get
            {
                if (string.IsNullOrEmpty(Description))
                {
                    return String.Empty;
                }

                if (Description.Length <= MaximumShortDescriptionLength)
                {
                    return Description;
                }

                return Description.Substring(0, MaximumShortDescriptionLength) + "...";
            }
        }
        public string Source { get; set; }

        public DateTimeOffset Created { get; set; }
        public bool IsInstalledSystem { get; set; }
        public bool IsInstalledInDirectory { get; set; }

        public string CreatedDisplay
        {
            get
            {
                if (Created.DateTime == DateTime.MinValue)
                {
                    return string.Empty;
                }

                return Created.ToString("d") + " " + Created.ToString("t");
            }
        }
    }
}
