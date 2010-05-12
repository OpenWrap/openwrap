using System.Collections.Generic;

namespace OpenRasta.Wrap.Resources
{
    public interface IWrapExport
    {
        string Name { get; }
        IEnumerable<IWrapExportItem> Items { get; }
    }
}