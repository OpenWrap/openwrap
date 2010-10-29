using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;

namespace OpenWrap.Exports
{
    public static class AssemblyReferenceExportExtensions
    {
        public static IEnumerable<IAssemblyReferenceExportItem> GetAssemblyReferences(this IPackageManager manager, bool includeContentOnly, ExecutionEnvironment exec, PackageDescriptor descriptor, params IPackageRepository[] repositories)
        {
            return GetAssemblyReferences(manager.TryResolveDependencies(descriptor, repositories), exec, includeContentOnly);
        }
        public static IEnumerable<IAssemblyReferenceExportItem> GetAssemblyReferences(this IPackageManager manager, ExecutionEnvironment exec, params IPackageRepository[] repositories)
        {
            return GetAssemblyReferencesFromPackages(repositories.SelectMany(x => x.PackagesByName.SelectMany(y => y)), exec);
        }

        static IEnumerable<IAssemblyReferenceExportItem> GetAssemblyReferences(DependencyResolutionResult resolveResult, ExecutionEnvironment exec, bool includeContentOnly)
        {
            var packages = resolveResult.Dependencies
                    .Where(x=> includeContentOnly || !IsInContentBranch(x))
                    .Select(x => x.Package);
            return GetAssemblyReferencesFromPackages(packages, exec);
        }

        static IEnumerable<IAssemblyReferenceExportItem> GetAssemblyReferencesFromPackages(IEnumerable<IPackageInfo> packages, ExecutionEnvironment exec)
        {
            return packages
                    .GroupBy(x=>x.Name)
                    .Select(x=>x.OrderByDescending(y=>y.Version).First())
                    .NotNull()
                    .SelectMany(x => x.Load().GetExport("bin", exec).Items)
                    .Cast<IAssemblyReferenceExportItem>();
        }

        static bool IsInContentBranch(ResolvedDependency resolvedDependency)
        {
            return resolvedDependency.Dependency.ContentOnly
                   || (resolvedDependency.Parent != null && IsInContentBranch(resolvedDependency.Parent));
        }
    }
}
