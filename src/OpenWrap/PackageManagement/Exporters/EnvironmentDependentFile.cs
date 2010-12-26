using System;

namespace OpenWrap.PackageManagement.Exporters
{
    public class EnvironmentDependentFile : IComparable<EnvironmentDependentFile>
    {
        public IExportItem Item;
        public string Platform;
        public string Profile;

        public int CompareTo(EnvironmentDependentFile other)
        {
            if (this.Profile != other.Profile)
            {
                return this.Profile.CompareTo(other.Profile) * -1;
            }
            if (Profile == other.Profile && Platform == other.Platform) return 0;
            if (Platform != "AnyCPU") return -1;
            return 1;
        }
    }
}