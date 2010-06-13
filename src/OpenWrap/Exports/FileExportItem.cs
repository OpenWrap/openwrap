using System.IO;
using OpenWrap.IO;

namespace OpenWrap.Exports
{
    public class FileExportItem : IExportItem
    {
        public string FullPath { get; set; }
        public FileExportItem(IFile filePath)
        {
            FullPath = filePath.Path.FullPath;
        }

    }
}