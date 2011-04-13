using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.PackageModel;

namespace OpenWrap.PackageManagement
{
    public class PackageGraphVisitor
    {
        Dictionary<string, IPackageInfo> _byName;
        ILookup<string, IPackageInfo> _dependents;

        public delegate bool PackageVisitor(IPackageInfo from, PackageDependency dependency, IPackageInfo to);
        public PackageGraphVisitor(IEnumerable<IPackageInfo> packages)
        {
            _byName = packages.ToDictionary(x => x.Identifier.Name, StringComparer.OrdinalIgnoreCase);

            _dependents = (
                                  from package in packages
                                  from dependency in package.Dependencies
                                  select new { package, dependency }
                          ).ToLookup(x => x.dependency.Name, x => x.package, StringComparer.OrdinalIgnoreCase);

        }
        public bool VisitFromLeafs(PackageVisitor visitor, IEnumerable<PackageDependency> dependencies = null)
        {
            var byDependents = new Dictionary<string, List<string>>();

            var nodes = (dependencies ?? Leafs()).ToList();

            nodes.ForEach(x => VisitDependencyNode(null, x, _byName[x.Name], (from, dep, to) =>
            {
                if (from == null) return true;
                List<string> dependents;
                if (!byDependents.TryGetValue(to.Name, out dependents))
                    byDependents[to.Name] = dependents = new List<string>();
                if (!dependents.Contains(from.Name))
                    dependents.Add(from.Name);
                return true;
            }, new Dictionary<string, int>()));
            var expectedVisits = byDependents.ToDictionary(x => x.Key, x => x.Value.Count);
            foreach (var package in nodes)
                if (!VisitDependencyNode(null, package, _byName[package.Name], visitor, expectedVisits))
                    return false;
            return true;
        }

        bool VisitDependencyNode(IPackageInfo from, PackageDependency dependency, IPackageInfo to, PackageVisitor visitor, Dictionary<string,int> visitsLeft)
        {
            if (!visitor(from, dependency, to)) return false;
            if (DecreaseVisitCount(visitsLeft, to.Name) <= 0)
            {
                foreach (var dependencyNode in to.Dependencies)
                    if (!VisitDependencyNode(to, dependencyNode, _byName[dependencyNode.Name], visitor, visitsLeft))
                        return false;
            }
            return true;
        }

        static int DecreaseVisitCount(Dictionary<string, int> visitsLeft, string name)
        {
            if (visitsLeft.ContainsKey(name) == false) return 0;
            return visitsLeft[name] = visitsLeft[name] - 1;
        }

        IEnumerable<PackageDependency> Leafs()
        {
            return _byName.Values
                    .Where(package => _dependents.Contains(package.Name) == false || _dependents[package.Name].Any() == false)
                    .Select(package=>new PackageDependencyBuilder(package.Name).Version(package.Version.ToString()).Build());

        }

    }
}