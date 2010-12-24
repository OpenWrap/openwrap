using System.Collections.Generic;
using System.Linq;
using OpenWrap.Collections;
using OpenWrap.PackageModel;

namespace OpenWrap.PackageManagement.DependencyResolvers
{
    public class PackageSelectionContext
    {
        readonly Stack<PackageResolveResults> _compatiblePackageVersions;
        readonly Stack<PackageResolveResults> _incompatiblePackages;

        public PackageSelectionContext()
        {
            _compatiblePackageVersions = new Stack<PackageResolveResults>();
            _compatiblePackageVersions.Push(new PackageResolveResults());
            _incompatiblePackages = new Stack<PackageResolveResults>();
            _incompatiblePackages.Push(new PackageResolveResults());
        }

        public PackageSelectionContext(PackageResolveResults incompatible)
        {
            _compatiblePackageVersions = new Stack<PackageResolveResults>();
            _compatiblePackageVersions.Push(new PackageResolveResults());
            _incompatiblePackages = new Stack<PackageResolveResults>();
            _incompatiblePackages.Push(new PackageResolveResults(incompatible));
        }

        public PackageResolveResults CompatiblePackageVersions
        {
            get { return _compatiblePackageVersions.Peek(); }
        }

        public PackageResolveResults IncompatiblePackageVersions
        {
            get { return _incompatiblePackages.Peek(); }
        }

        public void ExistingPackageCompatible(PackageIdentifier packageIdentifier, CallStack callStack)
        {
            _compatiblePackageVersions.Peek().Add(packageIdentifier, new[] { callStack });
        }

        public bool IsIgnored(PackageIdentifier identifier)
        {
            return IncompatiblePackageVersions[identifier].Failed.Count() > 0;
        }

        public void PackageConflicts(PackageIdentifier identifier, CallStack failingCallStack)
        {
            var existingCompatible = CompatiblePackageVersions[identifier];

            _compatiblePackageVersions.Pop();
            CompatiblePackageVersions.Remove(identifier);

            _incompatiblePackages.Pop();
            IncompatiblePackageVersions.Add(identifier, existingCompatible.Successful, existingCompatible.Failed.Concat(failingCallStack));
        }

        public void PackageHasChildrenConflicting(PackageIdentifier identifier)
        {
            _compatiblePackageVersions.Pop();
            CompatiblePackageVersions.Remove(identifier);

            var newIgnores = _incompatiblePackages.Pop();
            _incompatiblePackages.Pop();
            _incompatiblePackages.Push(newIgnores);
        }

        public void PackageSucceeds(PackageIdentifier packageNode, CallStack succeedingCallstack)
        {
            // commit the new ignore list
            var ignoredPackagesInBranch = _incompatiblePackages.Pop();
            _incompatiblePackages.Pop();
            _incompatiblePackages.Push(ignoredPackagesInBranch);

            // add successful package
            var foundPackages = _compatiblePackageVersions.Pop();
            foundPackages.Add(packageNode, successful: new[] { succeedingCallstack });
            _compatiblePackageVersions.Pop();
            _compatiblePackageVersions.Push(foundPackages);
        }

        public PackageIdentifier SelectedPackageByName(string packageName)
        {
            return CompatiblePackageVersions.FromName(packageName);
        }

        public void Trying(PackageIdentifier existing)
        {
            _compatiblePackageVersions.Push(new PackageResolveResults(CompatiblePackageVersions));
            CompatiblePackageVersions.Add(existing);
            _incompatiblePackages.Push(new PackageResolveResults(IncompatiblePackageVersions));
        }
    }
}