using System.IO;
using OpenRasta.Wrap.Resources;

namespace OpenRasta.Wrap.Repositories
{
    public class FileExportItem : IWrapExportItem
    {
        public string FullPath { get; set; }
        public FileExportItem(FileInfo filePath)
        {
            FullPath = filePath.FullName;
        }

    }
}