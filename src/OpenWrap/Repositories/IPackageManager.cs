using System;
using System.Collections.Generic;
using OpenWrap.Build.Services;
using OpenWrap.Dependencies;

namespace OpenWrap.Repositories
{
    public interface IPackageManager : IService
    {
        DependencyResolutionResult TryResolveDependencies(WrapDescriptor wrapDescriptor,
                                                          IPackageRepository projectRepository,
                                                          IPackageRepository userRepository,
                                                          IEnumerable<IPackageRepository> remoteRepositories);

        void UpdateDependency(ResolvedDependency dependency,
                              IPackageRepository destinationRepository);
    }
}