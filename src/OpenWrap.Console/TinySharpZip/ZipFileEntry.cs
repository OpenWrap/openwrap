using System;
using System.IO;

namespace TinySharpZip
{
    public class ZipFileEntry : ZipEntry
    {
        uint crc;
        Stream data;

        public ZipFileEntry(string fileName, Stream data)
        {
            this.FileName = fileName;
            this.data = data;
            ComputeCrc();
        }

        public uint Crc
        {
            get { return crc; }
        }

        public Stream Data
        {
            get { return data; }
            set
            {
                data = value;
                ComputeCrc();
            }
        }

        public string FileName { get; set; }

        void ComputeCrc()
        {
            if (data != null && data.Length != 0)
            {
                crc = Crc32.Compute(data);
            }
            else
            {
                crc = 0;
            }
        }
    }
}