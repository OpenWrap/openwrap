using System;
using OpenRasta.Wrap.Resources;

namespace OpenRasta.Wrap.Repositories
{
    public class EnvironmentDependentFile : IComparable<EnvironmentDependentFile>
    {
        public string Profile;
        public string Platform;
        public IWrapExportItem Item;
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