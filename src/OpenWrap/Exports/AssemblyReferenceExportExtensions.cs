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
        public static IEnumerable<IAssemblyReferenceExportItem> GetAssemblyReferences(this IPackageManager manager, ExecutionEnvironment exec, PackageDescriptor descriptor, params IPackageRepository[] repositories)
        {
            return GetAssemblyReferences(manager.TryResolveDependencies(descriptor, repositories), exec);
        }

        static IEnumerable<IAssemblyReferenceExportItem> GetAssemblyReferences(DependencyResolutionResult resolveResult, ExecutionEnvironment exec)
        {
            return resolveResult.Dependencies
                    .Select(x => x.Package)
                    .GroupBy(x=>x.Name)
                    .Select(x=>x.OrderByDescending(y=>y.Version).First())
                    .NotNull()
                    .SelectMany(x => x.Load().GetExport("bin", exec).Items)
                    .Cast<IAssemblyReferenceExportItem>();
        }
    }
}
