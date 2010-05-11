using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenRasta.Wrap.Resources
{
    public interface IWrap
    {
        string Name { get; set; }
        string Description { get; set; }
        string Uri { get; set; }
        Version Version { get; set; }
        IEnumerable<IWrapExport> Exports { get; }
    }

    public interface IWrapExport
    {
        string Name { get; }
        IEnumerable<IWrapExportItem> Items { get; }
    }
    public interface IWrapExportItem
    {
        string FullPath { get; }
    }
}
