using System;
using System.IO;

namespace OpenWrap.IO.Packaging
{
    public class PackageContent
    {
        public string FileName { get; set; }
        public string RelativePath { get; set; }

        public long? Size { get; set; }
        public Func<Stream> Stream { get; set; }
    }
}