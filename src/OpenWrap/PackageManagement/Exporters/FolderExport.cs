using System.Collections.Generic;

namespace OpenWrap.PackageManagement.Exporters
{
    public class FolderExport : IExport
    {
        public FolderExport(string folderName)
        {
            Name = folderName;
        }

        public IEnumerable<IExportItem> Items { get; set; }
        public string Name { get; set; }
    }
}