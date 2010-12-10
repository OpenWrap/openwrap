using System;
using OpenWrap.Windows.Framework;

namespace OpenWrap.Windows
{
    public class PackageViewModel : ViewModelBase
    {
        public string Name { get; set;  }
        public Version Version { get; set; }
        public string FullName { get; set; }
        public string Description { get; set; }
        public string GroupName { get; set; }

        public DateTimeOffset Created { get; set; }
        public bool Anchored { get; set; }
        public bool Nuked { get; set; }
    }
}
