using System.Collections.Generic;

namespace OpenWrap.PackageManagement
{
    public interface IExport
    {
        string Name { get; }
        IEnumerable<IExportItem> Items { get; }
    }
}