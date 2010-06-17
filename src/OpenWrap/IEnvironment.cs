using System.Collections.Generic;
using System.IO;
using OpenWrap.Build.Services;
using OpenWrap.Dependencies;
using OpenWrap.IO;
using OpenWrap.Repositories;

namespace OpenWrap
{
    public interface IEnvironment : IService
    {
        IPackageRepository ProjectRepository { get; }
        WrapDescriptor Descriptor { get; }
        IEnumerable<IPackageRepository> RemoteRepositories { get; }
        IPackageRepository SystemRepository { get; }
        IDirectory CurrentDirectory { get; }
        ExecutionEnvironment ExecutionEnvironment { get; }
    }
}