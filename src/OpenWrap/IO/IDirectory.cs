using System.Collections.Generic;

namespace OpenWrap.IO
{
    public interface IDirectory : IFileSystemItem<IDirectory>
    {
        IDirectory GetDirectory(string directoryPath);
        IFile GetFile(string fileName);
        IEnumerable<IFile> Files();
        IEnumerable<IDirectory> Directories();
        IEnumerable<IFile> Files(string filter);
        IEnumerable<IDirectory> Directories(string filter);
        void Add(IFile file);
    }
}