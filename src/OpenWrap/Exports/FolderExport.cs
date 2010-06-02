using System.Collections.Generic;
using System.IO;

namespace OpenWrap.Exports
{
    public class FolderExport : IExport
    {
        public FolderExport(DirectoryInfo folderPath)
        {
            Name = folderPath.Name;
        }

        public string Name
        {
            get;
            set;
        }

        public IEnumerable<IExportItem> Items
        {
            get;
            set;
        }
    }
}