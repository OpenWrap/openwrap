using System;
using System.IO;
using OpenRasta.IO;

namespace OpenRasta.Client
{
    public class ProgressStream : WrapperStream
    {
        readonly long _size;
        long _total = 0;
        readonly Action<int> _progressNotifier;

        public ProgressStream(long size,Action<int> progressNotifier, Stream underlyingStream)
                : base(underlyingStream)
        {
            _size = size;
            _progressNotifier = progressNotifier;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = base.Read(buffer, offset, count);
            NotifyProgress(read);
            return read;
        }

        void NotifyProgress(int amount)
        {
            _total += amount;

            _progressNotifier((int)(((double)_total / _size) * 100));
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            base.Write(buffer, offset, count);
            NotifyProgress(count);
        }
    }
}