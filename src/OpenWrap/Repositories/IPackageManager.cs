using System;
using System.Collections.Generic;
using OpenWrap.Dependencies;
using OpenWrap.Exports;
using OpenWrap.Services;

namespace OpenWrap.Repositories
{
    public interface IPackageManager : IService
    {
        IEnumerable<T> GetExports<T>(string exportName, ExecutionEnvironment environment, IEnumerable<IPackageRepository> repositories)
            where T : IExport;
        DependencyResolutionResult TryResolveDependencies(WrapDescriptor wrapDescriptor,
                                                          IEnumerable<IPackageRepository> repositoriesToQuery);

        void UpdateDependency(ResolvedDependency dependency,
                              ISupportPublishing destinationRepository);
    }
    
}