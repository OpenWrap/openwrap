using System.Collections.Generic;
using System.Linq;
using OpenWrap.PackageManagement.DependencyResolvers;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;
using OpenWrap.Runtime;

namespace OpenWrap.PackageManagement.Exporters.Assemblies
{
    public static class AssemblyReferences
    {
        public static IEnumerable<Exports.IAssembly> GetProjectAssemblyReferences(this IPackageManager manager,
                                                                                    IPackageDescriptor descriptor,
                                                                                    IPackageRepository repository,
                                                                                    ExecutionEnvironment environment,
                                                                                    bool includeContentOnly)
        {
            var lockedDescriptor = descriptor.Lock(repository);
            var sourcePackages = manager.ListProjectPackages(lockedDescriptor, repository);
            var assemblies = manager.GetProjectExports<Exports.IAssembly>(lockedDescriptor, repository, environment)
                    .SelectMany(x => x);

            if (includeContentOnly) return assemblies;

            var packagesToInclude = sourcePackages.NotInContentBranch(lockedDescriptor.Dependencies).Select(x=>x.Identifier).ToList();
            return assemblies.Where(x => packagesToInclude.Contains(x.Package.Identifier)).ToList();
        }

    }
}