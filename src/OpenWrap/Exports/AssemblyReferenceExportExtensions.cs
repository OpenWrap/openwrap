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
        public static IEnumerable<IAssemblyReferenceExportItem> GetAssemblyReferences(this IPackageResolver resolver, bool includeContentOnly, ExecutionEnvironment exec, PackageDescriptor descriptor, params IPackageRepository[] repositories)
        {
            return GetAssemblyReferences(resolver.TryResolveDependencies(descriptor, repositories), exec, includeContentOnly);
        }
        public static IEnumerable<IAssemblyReferenceExportItem> GetAssemblyReferences(this IPackageResolver resolver, ExecutionEnvironment exec, params IPackageRepository[] repositories)
        {
            return GetAssemblyReferencesFromPackages(repositories.SelectMany(x => x.PackagesByName.SelectMany(y => y)), exec);
        }

        static IEnumerable<IAssemblyReferenceExportItem> GetAssemblyReferences(DependencyResolutionResult resolveResult, ExecutionEnvironment exec, bool includeContentOnly)
        {
            var packages = resolveResult.ResolvedPackages.Where(resolvedPackage => includeContentOnly || !IsInContentBranch(resolvedPackage)).Select(x=>x.Package);
            return GetAssemblyReferencesFromPackages(packages, exec);
        }

        static bool IsInContentBranch(ResolvedPackage resolvedPackage)
        {
            return resolvedPackage.Dependencies.All(IsInContentBranch);
        }

        static bool IsInContentBranch(ParentedDependency resolvedPackage)
        {
            return resolvedPackage.Dependency.ContentOnly
                   || (resolvedPackage.Parent != null && IsInContentBranch(resolvedPackage.Parent));
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
    }
}
