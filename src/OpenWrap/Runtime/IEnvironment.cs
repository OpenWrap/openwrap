using System.Collections.Generic;
using OpenFileSystem.IO;
using OpenWrap.PackageModel;
using OpenWrap.PackageModel.Serialization;
using OpenWrap.Repositories;
using OpenWrap.Services;

namespace OpenWrap.Runtime
{
    public interface IEnvironment : IService
    {
        IPackageRepository ProjectRepository { get; }
        IPackageRepository CurrentDirectoryRepository { get; }
        IDictionary<string, FileBased<IPackageDescriptor>> ScopedDescriptors { get; }
        IFile DescriptorFile { get; }
        IPackageDescriptor Descriptor { get; }
        IPackageRepository SystemRepository { get; }
        IDirectory CurrentDirectory { get; }
        IDirectory ConfigurationDirectory { get; }
        ExecutionEnvironment ExecutionEnvironment { get; }
        IDirectory SystemRepositoryDirectory { get; }
    }
}