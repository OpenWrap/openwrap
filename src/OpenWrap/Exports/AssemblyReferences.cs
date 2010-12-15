using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;

namespace OpenWrap.Exports
{
    public static class AssemblyReferences
    {
        public static IEnumerable<IAssemblyReferenceExportItem> GetAssemblyReferences(this IPackageResolver resolver, bool includeContentOnly, ExecutionEnvironment exec, PackageDescriptor descriptor, params IPackageRepository[] repositories)
        {
            return GetAssemblyReferences(resolver.TryResolveDependencies(descriptor, repositories), exec, includeContentOnly);
        }
        public static IEnumerable<IAssemblyReferenceExportItem> GetAssemblyReferences(ExecutionEnvironment exec, params IPackageRepository[] repositories)
        {
            return GetAssemblyReferencesFromPackages(repositories.SelectMany(x => x.PackagesByName.SelectMany(y => y)), exec);
        }

        static IEnumerable<IAssemblyReferenceExportItem> GetAssemblyReferences(DependencyResolutionResult resolveResult, ExecutionEnvironment exec, bool includeContentOnly)
        {
            var packages = resolveResult.SuccessfulPackages.Where(resolvedPackage => includeContentOnly || !resolvedPackage.IsInContentBranch).SelectMany(x=>x.Packages);
            return GetAssemblyReferencesFromPackages(packages, exec);
        }


        static IEnumerable<IAssemblyReferenceExportItem> GetAssemblyReferencesFromPackages(IEnumerable<IPackageInfo> packages, ExecutionEnvironment exec)
        {
            return packages
                    .NotNull()
                    .GroupBy(x=>x.Name)
                    .Select(x=>x.OrderByDescending(y=>y.Version).First())
                    .NotNull()
                    .SelectMany(x => x.Load().GetExport("bin", exec).Items)
                    .Cast<IAssemblyReferenceExportItem>();
        }
    }
}
