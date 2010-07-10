using System.Collections.Generic;
using System.IO;
using OpenWrap.Dependencies;
using OpenWrap.IO;
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