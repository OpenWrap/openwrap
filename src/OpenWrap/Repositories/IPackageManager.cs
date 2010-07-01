using System;
using System.Collections.Generic;
using OpenWrap.Dependencies;
using OpenWrap.Services;

namespace OpenWrap.Repositories
{
    public interface IPackageManager : IService
    {
        DependencyResolutionResult TryResolveDependencies(WrapDescriptor wrapDescriptor,
                                                          IEnumerable<IPackageRepository> repositoriesToQuery);

        void UpdateDependency(ResolvedDependency dependency,
                              IPackageRepository destinationRepository);
    }
    
}