namespace OpenWrap.IO
{
    public abstract class AbstractFileSystem : IFileSystem
    {
        public abstract IDirectory CreateDirectory(IPath path);
        public abstract ITemporaryDirectory CreateTempDirectory();
        public abstract ITemporaryFile CreateTempFile();
        public abstract IDirectory GetDirectory(string directoryPath);
        public abstract IFile GetFile(string itemSpec);
        public abstract IPath GetPath(string path);
        public abstract IDirectory GetTempDirectory();
    }
}