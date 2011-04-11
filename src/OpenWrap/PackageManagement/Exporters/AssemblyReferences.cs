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

        public static IEnumerable<Exports.IAssembly> GetAssemblyReferences(this IPackageManager manager,
                                                                                    IPackageDescriptor descriptor,
                                                                                    IPackageRepository repository,
                                                                                    bool includeContentOnly)
        {
            var sourcePackages = manager.ListProjectPackages(descriptor, repository);
            var assemblies = manager.GetProjectExports<Exports.IAssembly>(descriptor, repository)
                    .SelectMany(x => x);

            if (!includeContentOnly) return assemblies;

            var packagesToInclude = sourcePackages.InContentBranch(descriptor.Dependencies).ToList();
            return assemblies.Where(x => packagesToInclude.Contains(x.Package));
        }

    }
}