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
        readonly IEnumerable<IPackageRepository> _repositories;
        readonly PackageSelectionContext _selectionContext;


        public DependencyVisitor(IEnumerable<IPackageRepository> repositories,
                                 PackageSelectionContext selectionContext,
                                 IEnumerable<PackageDependency> hints,
                                 IEnumerable<PackageNameOverride> nameOverrides)
        {
            Check.NotNull(selectionContext, "selectionContext");
            Check.NoNullElements(repositories, "repositories");
            Check.NoNullElements(hints, "hints");
            Check.NoNullElements(nameOverrides, "nameOverrides");

            _repositories = repositories;
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
            return _hints.ContainsKey(originalDependency.Name) && _hints[originalDependency.Name].VersionVertices.OfType<AnyVersionVertex>().Any() == false
                ? _hints[originalDependency.Name] : originalDependency;
        }

        PackageDependency ApplyPackageNameOverride(PackageDependency originalDependency)
        {
            return _nameOverrides.Aggregate(originalDependency, (modifiedDependency, wrapOverride) => wrapOverride.Apply(modifiedDependency));
        }

        KeyValuePair<PackageIdentifier, IPackageInfo>? NextAvailablePackageVersion(IEnumerable<PackageIdentifier> seen, PackageDependency dependency)
        {
            return (from packageById in from repo in _repositories
                                        from package in repo.FindAll(dependency)
                                        where _selectionContext.IsIgnored(package.Identifier) == false
                                        group package by package.Identifier
                    where seen.Contains(packageById.Key) == false && packageById.Count() > 0
                    orderby packageById.Key.Version descending
                    select new KeyValuePair<PackageIdentifier, IPackageInfo>?(
                            new KeyValuePair<PackageIdentifier, IPackageInfo>(
                                    packageById.Key,
                                    packageById.First())))
                    .FirstOrDefault();
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
            KeyValuePair<PackageIdentifier, IPackageInfo>? packageVersion;
            var seen = new List<PackageIdentifier>();
            while ((packageVersion = NextAvailablePackageVersion(seen, dependency)) != null)
            {
                seen.Add(packageVersion.Value.Key);
                _selectionContext.Trying(packageVersion.Value.Key);
                PushStack(packageVersion.Value.Key);

                IPackageInfo package = packageVersion.Value.Value;
                if (package != null && VisitDependencies(package.Dependencies))
                {
                    _selectionContext.PackageSucceeds(packageVersion.Value.Key, CurrentCallStack);
                    WriteDebug("dependency: version found");
                    PopStack();
                    return true;
                }
                _selectionContext.PackageHasChildrenConflicting(packageVersion.Value.Key);
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
                                         _selectionContext.CompatiblePackageVersions.Select(x => x.Key.ToString()).Join(","),
                                         _selectionContext.IncompatiblePackageVersions.Select(x => x.Key.ToString()).Join(","));

            Debug.WriteLine(new String(' ', _currentNode.Count) + text + packages);
        }
    }
}