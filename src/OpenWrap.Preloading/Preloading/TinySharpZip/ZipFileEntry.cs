using System.IO;

namespace OpenWrap.Preloading.TinySharpZip
{
    public class ZipFileEntry : ZipEntry
    {
        public ZipFileEntry(string fileName, Stream data)
        {
            this.FileName = fileName;
            Data = data;
        }

        public Stream Data { get; set; }

        public string FileName { get; private set; }
    }
}