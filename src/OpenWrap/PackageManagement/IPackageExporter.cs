using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenFileSystem.IO;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;
using OpenWrap.Runtime;
using OpenWrap.Services;

namespace OpenWrap.PackageManagement
{
    public interface IPackageExporter : IService
    {
        IEnumerable<IGrouping<string, TItems>> Exports<TItems>(IPackage package, ExecutionEnvironment environment) where TItems : IExportItem;
    }
}