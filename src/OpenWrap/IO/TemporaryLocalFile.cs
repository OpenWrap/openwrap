using System;

namespace OpenWrap.IO
{
    public class TemporaryLocalFile : LocalFile, ITemporaryFile
    {
        public TemporaryLocalFile(string filePath)
            : base(filePath)
        {
        }

        ~TemporaryLocalFile()
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