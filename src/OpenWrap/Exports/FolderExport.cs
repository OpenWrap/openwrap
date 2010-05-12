using System.Collections.Generic;
using System.IO;
using OpenRasta.Wrap.Resources;

namespace OpenRasta.Wrap.Repositories
{
    public class FolderExport : IWrapExport
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

        public IEnumerable<IWrapExportItem> Items
        {
            get;
            set;
        }
    }
}