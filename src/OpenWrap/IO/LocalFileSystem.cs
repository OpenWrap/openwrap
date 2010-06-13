using System.IO;

namespace OpenWrap.IO
{
    public class LocalFileSystem : AbstractFileSystem
    {
        public override IDirectory CreateDirectory(IPath path)
        {
            return new LocalDirectory(path.FullPath).Create();
        }

        public override ITemporaryDirectory CreateTempDirectory()
        {
            return new TemporaryLocalDirectory(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
        }

        public override ITemporaryFile CreateTempFile()
        {
            return new TemporaryLocalFile(Path.GetTempFileName());
        }

        public override IDirectory GetDirectory(string directoryPath)
        {
            return new LocalDirectory(directoryPath);
        }

        public override IFile GetFile(string itemSpec)
        {
            return new LocalFile(itemSpec);
        }

        public override IPath GetPath(string path)
        {
            return new LocalPath(path);
        }

        public override IDirectory GetTempDirectory()
        {
            return new LocalDirectory(Path.GetTempPath());
        }
    }
}