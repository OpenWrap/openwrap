namespace OpenWrap.IO
{
    public interface IPath
    {
        string FullPath { get; }
        IPath Combine(params string[] paths);
        IFileSystem FileSystem { get; }
    }
}