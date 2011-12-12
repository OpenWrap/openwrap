using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.PackageModel;

namespace OpenWrap.PackageManagement
{
    public class HooksStore
    {
        public event PackageChanged PackageAdded;
        public event PackageChanged PackageRemoved;
        public event PackageUpdated PackageUpdated;

        public HooksStore()
        {   
        }
        public IEnumerable<object> Installed(string repository, string packageName, SemanticVersion version, IEnumerable<IPackageInfo> packages)
        {
            var hooks = PackageAdded;
            if (hooks != null)
                return hooks.GetInvocationList().SelectMany(x => (IEnumerable<object>)x.DynamicInvoke(repository, packageName, version, packages));
            //return hooks.I((repository, packageName, version, packages);
            return Enumerable.Empty<object>();
        }
        public IEnumerable<object> Updated(string repository, string packageName, SemanticVersion fromVersion, SemanticVersion toVersion, IEnumerable<IPackageInfo> packages)
        {
            var hooks = PackageUpdated;
            if (hooks != null)
                return hooks.GetInvocationList().SelectMany(x => (IEnumerable<object>)x.DynamicInvoke(repository, packageName, fromVersion, toVersion, packages));

            return Enumerable.Empty<object>();
        }
        public IEnumerable<object> Removed(string repository, string packageName, SemanticVersion version, IEnumerable<IPackageInfo> packages)
        {
            var hooks = PackageRemoved;
            if (hooks != null)
                return hooks.GetInvocationList().SelectMany(x => (IEnumerable<object>)x.DynamicInvoke(repository, packageName, version, packages));

            return Enumerable.Empty<object>();
        }
    }
}