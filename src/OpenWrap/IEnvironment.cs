using System.Collections.Generic;
using OpenWrap.Build.Services;
using OpenWrap.Repositories;

namespace OpenWrap
{
    public interface IEnvironment : IService
    {
        IPackageRepository ProjectRepository { get; }
        string DescriptorPath { get; }
        IEnumerable<IPackageRepository> RemoteRepositories { get; }
        IPackageRepository UserRepository { get; }
    }
}