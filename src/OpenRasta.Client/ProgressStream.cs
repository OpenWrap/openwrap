﻿using System;
using System.IO;
using OpenRasta.IO;

namespace OpenRasta.Client
{
    public class ProgressStream : WrapperStream
    {
        readonly long _size;
        long _total = 0;
        readonly Action<TransferProgress> _progressNotifier;

        public ProgressStream(long size,Action<TransferProgress> progressNotifier, Stream underlyingStream)
                : base(underlyingStream)
        {
            _size = size;
            _progressNotifier = progressNotifier;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = base.Read(buffer, offset, count);
            
            NotifyProgress(read);

            if (read == 0)
                _progressNotifier(new TransferProgress(_total, _total, true));

            return read;
        }

        void NotifyProgress(int amount)
        {
            _total += amount;

            _progressNotifier(new TransferProgress(_total, _size));
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            base.Write(buffer, offset, count);
            NotifyProgress(count);
        }
    }
}