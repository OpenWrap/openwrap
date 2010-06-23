using System;

namespace OpenWrap.IO.FileSystem.Local
{
    public class TemporaryLocalDirectory : LocalDirectory, ITemporaryDirectory
    {
        public TemporaryLocalDirectory(string path)
            : base(path)
        {
            Create();
        }

        ~TemporaryLocalDirectory()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            Delete();
        }
    }
}