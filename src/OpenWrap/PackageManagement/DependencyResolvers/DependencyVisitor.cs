using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenWrap.Collections;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;

namespace OpenWrap.PackageManagement.DependencyResolvers
{
    public class DependencyVisitor
    {
        readonly Stack<Node> _currentNode = new Stack<Node>();
        readonly Dictionary<string, PackageDependency> _hints;
        readonly IEnumerable<PackageNameOverride> _nameOverrides;
        readonly List<KeyValuePair<PackageDependency, CallStack>> _notFound;
        readonly PackageSelectionContext _selectionContext;
        ILookup<string, IPackageInfo> _packages;


        public DependencyVisitor(ILookup<string,IPackageInfo> packages,
                                 PackageSelectionContext selectionContext,
                                 IEnumerable<PackageDependency> hints,
                                 IEnumerable<PackageNameOverride> nameOverrides)
        {
            Check.NotNull(selectionContext, "selectionContext");
            Check.NoNullElements(packages, "repositories");
            Check.NoNullElements(hints, "hints");
            Check.NoNullElements(nameOverrides, "nameOverrides");

            _packages = packages;
            _selectionContext = selectionContext;
            _nameOverrides = nameOverrides;
            _hints = hints.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
            _notFound = new List<KeyValuePair<PackageDependency, CallStack>>();
        }

        public ILookup<PackageDependency, CallStack> NotFound
        {
            get { return _notFound.ToLookup(x => x.Key, x => x.Value); }
        }

        CallStack CurrentCallStack
        {
            get { return new CallStack(_currentNode); }
        }

        public bool VisitDependencies(IEnumerable<PackageDependency> dependencies)
        {
                foreach (var dependency in dependencies)
                {
                    if (dependency == null)
                        throw new InvalidOperationException("One of the dependencies is null.");

                    if (!VisitDependency(dependency)) return false;
                }
                return true;
        }

        PackageDependency ApplyPackageHintOverride(PackageDependency originalDependency)
        {
            // TODO: Investigate being smarter in how overrides are applied
            return _hints.ContainsKey(originalDependency.Name) && _hints[originalDependency.Name] != new PackageDependency(originalDependency.Name)
                ? _hints[originalDependency.Name] : originalDependency;
        }

        PackageDependency ApplyPackageNameOverride(PackageDependency originalDependency)
        {
            return _nameOverrides.Aggregate(originalDependency, (modifiedDependency, wrapOverride) => wrapOverride.Apply(modifiedDependency));
        }

        IPackageInfo NextAvailablePackageVersion(IEnumerable<PackageIdentifier> seen, PackageDependency dependency)
        {
            var availablePackages = Match(dependency).Where(_=>!_selectionContext.IsIgnored(_.Identifier) && !seen.Contains(_.Identifier));
            return Select(availablePackages);
        }
        IPackageInfo Select(IEnumerable<IPackageInfo> availablePackages)
        {
            return availablePackages.OrderByDescending(_ => _.Identifier.Version).FirstOrDefault();
        }

        IEnumerable<IPackageInfo> Match(PackageDependency dependency)
        {
            return _packages.FindAll(dependency);
        }

        void PopStack()
        {
            _currentNode.Pop();
        }

        void PushStack(PackageIdentifier identifier)
        {
            _currentNode.Push(new PackageNode(identifier));
            WriteDebug("P:" + identifier);
        }

        void PushStack(PackageDependency dependency)
        {
            _currentNode.Push(new DependencyNode(dependency));
            WriteDebug("D:" + dependency);
        }

        bool VisitDependency(PackageDependency dependency)
        {
            try
            {
                dependency = ApplyPackageHintOverride(ApplyPackageNameOverride(dependency));
                PushStack(dependency);

                var existing = _selectionContext.SelectedPackageByName(dependency.Name);
                if (existing != null && existing.IsCompatibleWith(dependency))
                {
                    WriteDebug("current package selection matches dependency");
                    PushStack(existing);
                    _selectionContext.ExistingPackageCompatible(existing, CurrentCallStack);
                    PopStack();
                    return true;
                }
                if (existing != null)
                {
                    _selectionContext.Trying(existing);
                    _selectionContext.PackageConflicts(existing, CurrentCallStack);

                    WriteDebug("current package selection incompatible with dependency");
                    return false;
                }

                if (!VisitPackageVersions(dependency))
                {
                    //WriteDebug("VisitDependencies failed");
                    return false;
                }
                return true;
            }
            finally
            {
                PopStack();
            }
        }

        bool VisitPackageVersions(PackageDependency dependency)
        {
            IPackageInfo packageVersion;
            var seen = new List<PackageIdentifier>();
            while ((packageVersion = NextAvailablePackageVersion(seen, dependency)) != null)
            {
                seen.Add(packageVersion.Identifier);
                _selectionContext.Trying(packageVersion.Identifier);
                PushStack(packageVersion.Identifier);

                if (VisitDependencies(packageVersion.Dependencies))
                {
                    _selectionContext.PackageSucceeds(packageVersion.Identifier, CurrentCallStack);
                    WriteDebug("dependency: version found");
                    PopStack();
                    return true;
                }
                _selectionContext.PackageHasChildrenConflicting(packageVersion.Identifier);
                WriteDebug("dependency: version didn't match");
                PopStack();
            }
            WriteDebug("dependency: no version matches");
            if (seen.Empty())
            {
                _notFound.Add(new KeyValuePair<PackageDependency, CallStack>(dependency, CurrentCallStack));
            }
            return false;
        }

        void WriteDebug(string text)
        {
            var packages = string.Format(" Yes: {0} No:{1}",
                                         _selectionContext.CompatiblePackageVersions.Select(x => x.Key.ToString()).JoinString(","),
                                         _selectionContext.IncompatiblePackageVersions.Select(x => x.Key.ToString()).JoinString(","));

            Debug.WriteLine(new String(' ', _currentNode.Count) + text + packages);
        }
    }
}