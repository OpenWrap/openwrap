using System;
using System.Collections.Generic;
using OpenWrap.Dependencies;
using OpenWrap.Services;

namespace OpenWrap.Repositories
{
    public interface IPackageResolver : IService
    {
        DependencyResolutionResult TryResolveDependencies(PackageDescriptor packageDescriptor,
                                                          IEnumerable<IPackageRepository> repositoriesToQuery);

    }
}