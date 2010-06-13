namespace OpenWrap.IO
{
    public interface IFileSystemItem<T> : IFileSystemItem
        where T : IFileSystemItem
    {
        T Create();
    }

    public interface IFileSystemItem
    {
        IPath Path { get; }
        IDirectory Parent { get; }
        IFileSystem FileSystem { get; }
        bool Exists { get; }
        string Name { get; }
        void Delete();
    }
}