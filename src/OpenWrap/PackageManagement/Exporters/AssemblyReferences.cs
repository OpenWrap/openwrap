using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Collections;
using OpenWrap.PackageManagement.DependencyResolvers;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;
using OpenWrap.Runtime;

namespace OpenWrap.PackageManagement.Exporters
{
    public static class AssemblyReferences
    {
        public static IEnumerable<IAssemblyReferenceExportItem> GetAssemblyReferences(this IPackageResolver resolver,
                                                                                      bool includeContentOnly,
                                                                                      ExecutionEnvironment exec,
                                                                                      IPackageDescriptor descriptor,
                                                                                      params IPackageRepository[] repositories)
        {
            return GetAssemblyReferences(resolver.TryResolveDependencies(descriptor, repositories.NotNull()), exec, includeContentOnly);
        }

        public static IEnumerable<IAssemblyReferenceExportItem> GetAssemblyReferences(ExecutionEnvironment exec, params IPackageRepository[] repositories)
        {
            return GetAssemblyReferencesFromPackages(repositories.NotNull().SelectMany(x => x.PackagesByName.SelectMany(y => y)), exec);
        }

        static IEnumerable<IAssemblyReferenceExportItem> GetAssemblyReferences(DependencyResolutionResult resolveResult, ExecutionEnvironment exec, bool includeContentOnly)
        {
            var packages = resolveResult.SuccessfulPackages.Where(resolvedPackage => includeContentOnly || !resolvedPackage.IsInContentBranch).SelectMany(x => x.Packages);
            return GetAssemblyReferencesFromPackages(packages, exec);
        }


        static IEnumerable<IAssemblyReferenceExportItem> GetAssemblyReferencesFromPackages(IEnumerable<IPackageInfo> packages, ExecutionEnvironment exec)
        {
            return from packageInfo in packages.NotNull()
                           .GroupBy(x => x.Name)
                           .Select(x => x.OrderByDescending(y => y.Version).First())
                   let package = packageInfo.Load()
                   from assembly in package.Load().GetExport("bin", exec).Items.Cast<IAssemblyReferenceExportItem>()
                   where MatchesReferenceSection(package, assembly)
                   select assembly;
        }

        static bool MatchesReferenceSection(IPackage package, IAssemblyReferenceExportItem assembly)
        {
            var specs = package.Descriptor.ReferencedAssemblies.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                                                               .Select(spec=>spec.Trim().Wildcard());
            var fileName = System.IO.Path.GetFileName(assembly.FullPath);
            return specs.Any(spec => spec.IsMatch(assembly.AssemblyName.Name) || spec.IsMatch(fileName));
        }
    }
}