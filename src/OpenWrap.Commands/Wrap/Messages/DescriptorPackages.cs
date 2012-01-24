using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.PackageModel;

namespace OpenWrap.Commands.Wrap
{
    public class DescriptorPackages : Info
    {
        public string DescriptorName { get; set; }
        public IEnumerable<Package> Packages { get; set; }

        public bool IncludeDependencies { get; private set; }

        public DescriptorPackages(string descriptorName,
                                  IEnumerable<PackageDependency> dependencies, 
                                  IEnumerable<IPackageInfo> resolvedPackages,
                                  bool includeDependencies,
                                  Func<string,bool> nameSelector = null,
                                  IEnumerable<IPackageInfo> lockedPackages = null) : base()
        {
            IncludeDependencies = includeDependencies;
            lockedPackages = lockedPackages ?? Enumerable.Empty<IPackageInfo>();
            DescriptorName = descriptorName == string.Empty
                                 ? "default scope"
                                 : descriptorName + " scope";
            
            var recursionDefender = new Stack<string>();
            var includeChildren = includeDependencies || nameSelector != null;
            Packages = dependencies
                .Select(_ => 
                        CreatePackageInfo(_, resolvedPackages, lockedPackages, includeChildren, recursionDefender))
                .ToList();
            if (nameSelector != null)
                Packages = FilterByName(Packages, nameSelector);
        }

        
        IEnumerable<Package> FilterByName(IEnumerable<Package> packages, Func<string, bool> nameSelector)
        {
            return packages.Where(_ => Prune(_, nameSelector));
        }

        bool Prune(Package pack, Func<string, bool> nameSelector)
        {
            if (nameSelector(pack.PackageInfo.Name))
            {
                pack.Children = Truncated;
                return true;
            }
            List<Package> newChildren = new List<Package>();
            bool success = false;
            foreach(var child in pack.Children)
            {

                if (Prune(child, nameSelector))
                {
                    success = true;
                    newChildren.Add(child);
                }
            }
            pack.Children = newChildren;
            return success;
        }

        public static readonly ICollection<Package> Truncated = new List<Package>().AsReadOnly();
        Package CreatePackageInfo(PackageDependency dependency, IEnumerable<IPackageInfo> resolvedPackages, IEnumerable<IPackageInfo> lockedPackages, bool includeChildren, Stack<string> recursionDefender)
        {

            var packageInfo = new Package(
                resolvedPackages.FirstOrDefault(
                    pack => pack.Name.EqualsNoCase(dependency.Name)),
                dependency.ToString(),
                lockedPackages.Any(x => x.Name.EqualsNoCase(dependency.Name)));
            if (!includeChildren) return packageInfo;
            if (recursionDefender.ContainsNoCase(dependency.Name))
                packageInfo.Children = Truncated;
            else
            {
                recursionDefender.Push(dependency.Name);
                packageInfo.Children = (
                                           packageInfo.PackageInfo.Dependencies.Select(
                                               dep => CreatePackageInfo(dep, resolvedPackages, lockedPackages, includeChildren, recursionDefender)))
                    .ToList();
                recursionDefender.Pop();
            }
            return packageInfo;
        }

        public class Package
        {
            public Package(IPackageInfo packageInfo, string spec, bool locked)
            {
                PackageInfo = packageInfo;
                Spec = spec;
                Locked = locked;
                Children = new Package[0];
            }

            public IPackageInfo PackageInfo { get; set; }
            public string Spec { get; set; }
            public bool Locked { get; set; }
            public ICollection<Package> Children { get; set; }
        }
        public override string ToString()
        {
            var tree = new TreeRenderer();

            tree.PrintLine("─" + DescriptorName);
            PrintPackages(tree);

            return tree.Content;
        }

        void PrintPackages(TreeRenderer tree)
        {
            Packages.Select(_ => new Node<Package>(_, PrintPackage))
                .Cast<Node>()
                .Render(tree);
        }

        void PrintPackage(TreeRenderer tree, Package package)
        {
            var packageName = package.PackageInfo.Name + " " + package.PackageInfo.SemanticVersion;
            if (package.PackageInfo.Title != null)
                packageName += " (" + package.PackageInfo.Title + ")";
            if (package.Locked)
                packageName += " [locked]";
            var dependsLine = "depends: " + package.Spec;

            tree.PrintLine(IncludeDependencies ? dependsLine : packageName);

            var childNodes = package.Children == Truncated
                                 ? new[] { new Node<string>("...", (t, val) => t.PrintLine(val)) }
                                 : package.Children.Select(_ => new Node<Package>(_, PrintPackage)).Cast<Node>();
            if (!IncludeDependencies) childNodes.Render(tree);
            else
            {
                
                new[]{new Node<string>(packageName,
                                       (t, val) =>
                                       {
                                           t.PrintLine(val);
                                           childNodes.Render(t);
                                       })}.Render(tree);
            }
        }


    }
}