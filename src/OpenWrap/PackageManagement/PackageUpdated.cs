using System;
using System.Collections.Generic;
using OpenWrap.PackageModel;

namespace OpenWrap.PackageManagement
{
    public delegate IEnumerable<object> PackageUpdated(string repository, string name, Version fromVersion, Version toVersion, IEnumerable<IPackageInfo> packages);
}