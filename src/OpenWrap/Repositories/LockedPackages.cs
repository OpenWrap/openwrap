using System.Collections.Generic;

namespace OpenWrap.Repositories
{
    public class LockedPackages
    {
        public LockedPackages()
        {
            Lock = new List<LockedPackage>();

        }
        public IList<LockedPackage> Lock { get; set; }
    }
}