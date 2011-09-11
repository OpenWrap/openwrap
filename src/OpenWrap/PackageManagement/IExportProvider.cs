using System.Collections.Generic;
using System.Linq;
using OpenWrap.PackageModel;
using OpenWrap.Runtime;

namespace OpenWrap.PackageManagement
{
    public interface IExportProvider
    {
        IEnumerable<IGrouping<string, TItem>> Items<TItem>(IPackage package, ExecutionEnvironment environment) where TItem : IExportItem;
    }
}