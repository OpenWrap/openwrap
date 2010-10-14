using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenWrap.Tests
{
    public class NonSeekableMemoryStream : MemoryStream
    {
        public NonSeekableMemoryStream(byte[] data) : base(data)
        {
        }

        public override bool CanSeek
        {
            get { return false; }
        }
        public override long Seek(long offset, SeekOrigin loc)
        {
            throw new NotSupportedException();
        }
    }
}
