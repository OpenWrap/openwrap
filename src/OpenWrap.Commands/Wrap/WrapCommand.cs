using System.Collections.Generic;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;

namespace OpenWrap.Commands.Wrap
{
    public abstract class WrapCommand : AbstractCommand
    {
        public IEnvironment Environment
        {
            get { return Services.Services.GetService<IEnvironment>(); }

        }

        protected IPackageResolver PackageResolver
        {
            get { return Services.Services.GetService<IPackageResolver>(); }
        }

        protected DependencyResolutionResult ResolveDependencies(PackageDescriptor packageDescriptor, IEnumerable<IPackageRepository> repos)
        {
            return PackageResolver.TryResolveDependencies(packageDescriptor, repos);
        }
    }
}