using System.Collections.Generic;

namespace OpenWrap.Exports
{
    public interface IExport
    {
        string Name { get; }
        IEnumerable<IExportItem> Items { get; }
    }
}