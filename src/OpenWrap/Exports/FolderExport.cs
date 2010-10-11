using System.Collections.Generic;
using System.IO;
using OpenFileSystem.IO;

namespace OpenWrap.Exports
{
    public class FolderExport : IExport
    {
        public FolderExport(string folderName)
        {
            Name = folderName;
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
