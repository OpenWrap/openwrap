using System.Collections.Generic;
using OpenFileSystem.IO;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;
using OpenWrap.Services;

namespace OpenWrap.Runtime
{
    public interface IEnvironment : IService
    {
        IPackageRepository ProjectRepository { get; }
        IPackageRepository CurrentDirectoryRepository { get; }
        IFile DescriptorFile { get; }
        PackageDescriptor Descriptor { get; }
        IEnumerable<IPackageRepository> RemoteRepositories { get; }
        IPackageRepository SystemRepository { get; }
        IDirectory CurrentDirectory { get; }
        IDirectory ConfigurationDirectory { get; }
        ExecutionEnvironment ExecutionEnvironment { get; }
    }
}