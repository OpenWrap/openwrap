using System;
using System.IO;

namespace OpenWrap.IO.FileSystem.Local
{
    public class LocalFileSystem : AbstractFileSystem
    {
        public override IDirectory CreateDirectory(string path)
        {
            return new LocalDirectory(path).Create();
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
            return new LocalDirectory(Path.GetFullPath(Path.Combine(Environment.CurrentDirectory,directoryPath)));
        }

        public override IFile GetFile(string filePath)
        {
            return new LocalFile(Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, filePath)));
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