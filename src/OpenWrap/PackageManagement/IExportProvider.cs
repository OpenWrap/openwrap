using System.Collections.Generic;
using System.Linq;
using OpenWrap.PackageModel;

namespace OpenWrap.PackageManagement
{
    public interface IExportProvider
    {
        IEnumerable<IGrouping<string, TItem>> Items<TItem>(IPackage pacakge) where TItem : IExportItem;
    }
}