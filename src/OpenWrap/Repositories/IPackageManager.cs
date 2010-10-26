using System;
using System.Collections.Generic;
using OpenWrap.Dependencies;
using OpenWrap.Exports;
using OpenWrap.Services;

namespace OpenWrap.Repositories
{
    public interface IPackageManager : IService
    {
        /// <summary>
        /// Gets all the exports present in the provided repositories.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="exportName"></param>
        /// <param name="environment"></param>
        /// <param name="repositories"></param>
        /// <returns></returns>
        IEnumerable<T> GetExports<T>(string exportName, ExecutionEnvironment environment, IEnumerable<IPackageRepository> repositories)
            where T : IExport;
        DependencyResolutionResult TryResolveDependencies(PackageDescriptor packageDescriptor,
                                                          IEnumerable<IPackageRepository> repositoriesToQuery);

        void UpdateDependency(ResolvedDependency dependency,
                              ISupportPublishing destinationRepository);
    }
    
}