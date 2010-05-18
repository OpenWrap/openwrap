using System.IO;

namespace OpenWrap.Exports
{
    public class FileExportItem : IExportItem
    {
        public string FullPath { get; set; }
        public FileExportItem(FileInfo filePath)
        {
            FullPath = filePath.FullName;
        }

    }
}