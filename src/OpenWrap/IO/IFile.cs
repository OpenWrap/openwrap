using System.IO;

namespace OpenWrap.IO
{
    public interface IFile : IFileSystemItem<IFile>
    {
        string NameWithoutExtension { get; }
        Stream Open(FileMode fileMode, FileAccess fileAccess, FileShare fileShare);
    }
}