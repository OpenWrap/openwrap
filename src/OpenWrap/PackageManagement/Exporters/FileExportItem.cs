using OpenFileSystem.IO;

namespace OpenWrap.PackageManagement.Exporters
{
    public class FileExportItem : IExportItem
    {
        public FileExportItem(IFile filePath)
        {
            FullPath = filePath.Path.FullPath;
        }

        public string FullPath { get; set; }
    }
}