using System.Collections.Generic;
using OpenWrap.Build.Services;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;

namespace OpenWrap
{
    public interface IEnvironment : IService
    {
        IPackageRepository ProjectRepository { get; }
        WrapDescriptor Descriptor { get; }
        IEnumerable<IPackageRepository> RemoteRepositories { get; }
        IPackageRepository UserRepository { get; }
    }
}