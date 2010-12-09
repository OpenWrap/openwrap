using System;
using System.IO;

namespace OpenWrap.Dependencies
{
    public class PackageContent
    {
        public string RelativePath { get; set; }
        public string FileName { get; set; }
        public Func<Stream> Stream { get;set; }

        public long? Size { get; set; }
    }
}