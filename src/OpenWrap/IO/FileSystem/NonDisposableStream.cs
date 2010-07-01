using System.IO;

namespace OpenWrap.IO.FileSystem
{
    public class NonDisposableStream : Stream
    {
        public NonDisposableStream(Stream innerStream)
        {
            InnerStream = innerStream;
        }

        public Stream InnerStream { get; private set; }

        public override void Close()
        {
            InnerStream.Position = 0;
        }

        protected override void Dispose(bool disposing)
        {
            InnerStream.Position = 0;
        }

        public override void Flush()
        {
            InnerStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return InnerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            InnerStream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return InnerStream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            InnerStream.Write(buffer, offset, count);
        }

        public override bool CanRead
        {
            get { return InnerStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return InnerStream.CanSeek; }
        }
        public override bool CanWrite
        {
            get { return InnerStream.CanWrite; }
        }

        public override long Length
        {
            get { return InnerStream.Length; }
        }

        public override long Position
        {
            get { return InnerStream.Position; }
            set { InnerStream.Position = value; }
        }
    }
}