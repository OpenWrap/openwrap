using System;
using System.Collections.Generic;
using OpenWrap.PackageModel;

namespace OpenWrap.PackageManagement
{
    public delegate IEnumerable<object> PackageUpdated(string repository, string name, SemanticVersion fromVersion, SemanticVersion toVersion, IEnumerable<IPackageInfo> packages);
}