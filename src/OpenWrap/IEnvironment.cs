using System.Collections.Generic;
using OpenWrap.Dependencies;
using OpenFileSystem.IO;
using OpenWrap.Repositories;
using OpenWrap.Services;

namespace OpenWrap
{
    public interface IEnvironment : IService
    {
        IPackageRepository ProjectRepository { get; }
        IPackageRepository CurrentDirectoryRepository { get; }
        WrapDescriptor Descriptor { get; }
        IEnumerable<IPackageRepository> RemoteRepositories { get; }
        IPackageRepository SystemRepository { get; }
        IDirectory CurrentDirectory { get; }
        IDirectory ConfigurationDirectory { get; }
        ExecutionEnvironment ExecutionEnvironment { get; }
    }
}