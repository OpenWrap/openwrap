namespace OpenWrap.IO
{
    public interface IFileSystem
    {
        IDirectory GetDirectory(string directoryPath);
        IPath GetPath(string path);

        ITemporaryDirectory CreateTempDirectory();
        IDirectory CreateDirectory(string path);
        IFile GetFile(string itemSpec);
        ITemporaryFile CreateTempFile();
        IDirectory GetTempDirectory();
    }
}