using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;
using OpenWrap.Repositories.Http;

namespace OpenWrap.PackageManagement.DependencyResolvers
{
    public class StrategyResolver : IPackageResolver
    {
        const int MAX_RETRIES = 5000;

        public DependencyResolutionResult TryResolveDependencies(IPackageDescriptor packageDescriptor, IEnumerable<IPackageRepository> repositoriesToQuery)
        {
            Debug.WriteLine("Resolving descriptor " + packageDescriptor.Dependencies.Select(x => x.ToString()).JoinString(", "));

            var excluded = new List<IPackageInfo>();

            var allPackages = repositoriesToQuery.Packages().SelectMany(_ => _).ToList();
            var notFound = new List<PackageDependency>();

            for (int i = 0; i < MAX_RETRIES; i++)
            {
                var visitor = new LoggingPackageResolver(
                    allPackages,
                    packages => Nuke(PackageStrategy.Latest(packages)),
                    fail: excluded);

                visitor.ReadDependency = Override(packageDescriptor.Overrides, visitor.ReadDependency);

                if (visitor.Visit(packageDescriptor.Dependencies))
                {
                    return BuildResult(packageDescriptor, visitor, allPackages);
                }

                var hasNewIncompatibles = visitor.IncompatiblePackages.Except(excluded).Any();

                var currentNotFound = ReadNotFoundDependencies(visitor);
                var hasNewNotFound = currentNotFound.Except(notFound).Any();
                if (!hasNewNotFound && !hasNewIncompatibles)
                    return BuildResult(packageDescriptor, visitor, allPackages);
                excluded.AddRange(visitor.IncompatiblePackages);
                notFound = currentNotFound.ToList();
            }

            throw new InvalidOperationException(string.Format("OpenWrap tried {0} times to resolve the tree of dependencies and gave up.", MAX_RETRIES));
        }

        Func<PackageDependency, Func<IPackageInfo, bool?>> Override(ICollection<PackageNameOverride> overrides, Func<PackageDependency, Func<IPackageInfo, bool?>> readDependency)
        {
            var dic = overrides.ToDictionary(_ => _.OldPackage, _ => _.NewPackage, StringComparer.OrdinalIgnoreCase);

            return dependency => dic.ContainsKey(dependency.Name)
                                     ? readDependency(new PackageDependencyBuilder(dependency).Name(dic[dependency.Name]))
                                     : readDependency(dependency);
        }

        IPackageInfo WrapPackageWithName(IPackageInfo packageInfo, string newPackageName)
        {
            return new PackageWrapper(packageInfo, newPackageName);
        }

        IEnumerable<IPackageInfo> Nuke(IEnumerable<IPackageInfo> packages)
        {
            return packages.Where(_ => _.Nuked == false).Concat(packages.Where(_ => _.Nuked));
        }

        DependencyResolutionResult BuildResult(IPackageDescriptor packageDescriptor, LoggingPackageResolver visitor, List<IPackageInfo> allPackages)
        {
            return new DependencyResolutionResult(packageDescriptor,
                                                  FromPacks(allPackages, visitor.Success).ToList(),
                                                  FromPacks(allPackages, visitor.Fail).ToList(),
                                                  FromNotFound(visitor.NotFound).ToList());
        }

        static IEnumerable<PackageDependency> ReadNotFoundDependencies(LoggingPackageResolver visitor)
        {
            return visitor.NotFound.Select(_ => ((DependencyNode)_.Last()).Dependency);
        }

        public void Initialize()
        {
        }

        IEnumerable<ResolvedPackage> FromNotFound(List<CallStack> success)
        {
            if (success.Count == 0) return Enumerable.Empty<ResolvedPackage>();

            var groups = success.GroupBy(
                stack => ((DependencyNode)stack.Last()).Dependency.Name, 
                StringComparer.OrdinalIgnoreCase);

            return from byPackage in groups
                   select new ResolvedPackage(
                       new PackageIdentifier(byPackage.Key), 
                       Enumerable.Empty<IPackageInfo>(), 
                       byPackage);
        }

        IEnumerable<ResolvedPackage> FromPacks(IEnumerable<IPackageInfo> allPackages, List<CallStack> success)
        {
            if (success.Count == 0) return Enumerable.Empty<ResolvedPackage>();

            var groups = success
                .Select(_ => new { last = _.Last() as PackageNode, entry = _ })
                .Where(_ => _.last != null)
                .GroupBy(
                    stack => stack.last.Identifier.Name, 
                    stack => stack.entry, 
                    StringComparer.OrdinalIgnoreCase);

            return from byPackage in groups
                   let lastNodes = byPackage.Select(_ => ((PackageNode)_.Last()).Identifier).Distinct()
                   let packages = lastNodes.Select(id => allPackages.First(_ => _.Identifier == id))
                   let resolvedId = lastNodes.Count() == 1 
                                    ? lastNodes.Single()
                                    : new PackageIdentifier(byPackage.Key)
                   select new ResolvedPackage(
                       resolvedId, 
                       packages, 
                       byPackage);
        }
    }

    class PackageWrapper : IPackageInfo
    {
        readonly IPackageInfo _packageInfo;

        public PackageWrapper(IPackageInfo packageInfo, string newPackageName)
        {
            _packageInfo = packageInfo;
            Name = newPackageName;
        }

        public PackageIdentifier Identifier
        {
            get { return new PackageIdentifier(Name, SemanticVersion); }
        }

        public ICollection<PackageDependency> Dependencies
        {
            get { return _packageInfo.Dependencies; }
        }

        public string Name { get; private set; }

        public SemanticVersion SemanticVersion
        {
            get { return _packageInfo.SemanticVersion; }
        }

        public Version Version
        {
            get { return _packageInfo.Version; }
        }

        public IPackageRepository Source
        {
            get { return _packageInfo.Source; }
        }

        public string FullName
        {
            get { return _packageInfo.FullName; }
        }

        public string Description
        {
            get { return _packageInfo.Description; }
        }

        public string Title
        {
            get { return _packageInfo.Title; }
        }

        public DateTimeOffset Created
        {
            get { return _packageInfo.Created; }
        }

        public bool Anchored
        {
            get { return _packageInfo.Anchored; }
        }

        public bool Nuked
        {
            get { return _packageInfo.Nuked; }
        }

        public bool IsValid
        {
            get { return _packageInfo.IsValid; }
        }

        public IPackage Load()
        {
            return _packageInfo.Load();
        }
    }
}