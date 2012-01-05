using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using OpenWrap.PackageModel;

namespace OpenWrap.PackageManagement
{
    public class PackageGraphVisitor
    {
        readonly Dictionary<string, IPackageInfo> _byName;
        readonly ILookup<string, IPackageInfo> _dependents;

        public PackageGraphVisitor(IEnumerable<IPackageInfo> packages)
        {
            _byName = packages.ToDictionary(x => x.Identifier.Name, StringComparer.OrdinalIgnoreCase);

            _dependents = (
                                  from package in packages
                                  from dependency in package.Dependencies
                                  select new { package, dependency }
                          ).ToLookup(x => x.dependency.Name, x => x.package, StringComparer.OrdinalIgnoreCase);
        }

        public delegate bool PackageVisitor(IPackageInfo from, PackageDependency dependency, IPackageInfo to);

        public bool VisitFrom(PackageVisitor visitor)
        {
            return VisitFrom(null, visitor);
        }

        public bool VisitFrom(IEnumerable<PackageDependency> dependencies, PackageVisitor visitor)
        {
            var nodes = (dependencies ?? Leafs()).Where(x=>_byName.ContainsKey(x.Name)).ToList();

            var expectedVisits = GetExpectedVisitCounts(nodes).ToDictionary(x => x.Key, x => x.Value.Count);
            return nodes.All(package => _byName.ContainsKey(package.Name) && VisitDependencyNode(null, package, _byName[package.Name], visitor, expectedVisits));
        }

        Dictionary<string, List<string>> GetExpectedVisitCounts(List<PackageDependency> nodes)
        {
            var byDependents = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            nodes.ForEach(x => VisitDependencyNode(null,
                                                   x,
                                                   _byName[x.Name],
                                                   (from, dep, to) =>
                                                   {
                                                       if (from == null) return true;
                                                       if (from.Name == to.Name) return true;
                                                       List<string> dependents;
                                                       if (!byDependents.TryGetValue(to.Name, out dependents))
                                                           byDependents[to.Name] = dependents = new List<string>();
                                                       if (!dependents.Contains(from.Name))
                                                           dependents.Add(from.Name);
                                                       return true;
                                                   },
                                                   new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)));
            return byDependents;
        }

        static int DecreaseVisitCount(Dictionary<string, int> visitsLeft, string name)
        {
            return visitsLeft.ContainsKey(name) == false ? (visitsLeft[name] = 0) : (visitsLeft[name] = visitsLeft[name] - 1);
        }

        IEnumerable<PackageDependency> Leafs()
        {
            var leafs = _byName.Values
                    .Where(package => _dependents.Contains(package.Name) == false || _dependents[package.Name].Any() == false)
                    .Select(package => new {dep=new PackageDependency(package.Name), package});
            var visited = leafs.SelectMany(x => VisitAll(x.package)).ToList();
            

            var unvisited = _byName.Values.Where(x => visited.Contains(x.Name) == false).ToList();
            var recursives = new List<IPackageInfo>();

            IPackageInfo current;
            while ((current = unvisited.FirstOrDefault()) != null)
            {
                recursives.Add(current);
                var seen = VisitAll(current);
                unvisited.RemoveAll(x => seen.Contains(x.Name));
            }
            return leafs.Select(_ => _.dep).Concat(recursives.Select(_ => new PackageDependency(_.Name)));
        }
        IEnumerable<string> VisitAll(IPackageInfo root)
        {
            var names = new List<string>();
            VisitDependencyNode(null, new PackageDependency(root.Name), root, (from, dep, to) => AddReturn(names, to.Name, true), new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase));
            return names;
        }
        bool VisitDependencyNode(IPackageInfo from, PackageDependency dependency, IPackageInfo to, PackageVisitor visitor, Dictionary<string, int> visitsLeft)
        {
            var visitResult = visitor(from, dependency, to);
            return visitResult &&
                   (DecreaseVisitCount(visitsLeft, to.Name) != 0 ||
                    to.Dependencies.All(dependencyNode => !_byName.ContainsKey(dependencyNode.Name) || VisitDependencyNode(to, dependencyNode, _byName[dependencyNode.Name], visitor, visitsLeft)));
        }

        TReturn AddReturn<T, TReturn>(ICollection<T> collection, T value, TReturn returnValue)
        {
            collection.Add(value);
            return returnValue;
        }
    }
}