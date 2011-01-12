using System.Collections.Generic;
using OpenWrap.PackageManagement.DependencyResolvers;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;
using OpenWrap.Services;

namespace OpenWrap.PackageManagement
{
    public interface IPackageResolver : IService
    {
        DependencyResolutionResult TryResolveDependencies(IPackageDescriptor packageDescriptor, IEnumerable<IPackageRepository> repositoriesToQuery);
    }
}