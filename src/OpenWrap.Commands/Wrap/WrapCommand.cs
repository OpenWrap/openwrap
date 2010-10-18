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

        protected IPackageManager PackageManager
        {
            get { return Services.Services.GetService<IPackageManager>(); }
        }

        protected DependencyResolutionResult ResolveDependencies(PackageDescriptor packageDescriptor, IEnumerable<IPackageRepository> repos)
        {
            return PackageManager.TryResolveDependencies(packageDescriptor, repos);
        }
    }
}