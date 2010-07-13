using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;

namespace OpenWrap.Exports
{
    public static class AssemblyReferenceExportExtensions
    {
        public static IEnumerable<IAssemblyReferenceExportItem> GetAssemblyReferences(this IPackageManager manager, ExecutionEnvironment environment, params IPackageRepository[] repositories)
        {
            return manager.GetExports<IExport>("bin", environment, repositories.NotNull()).SelectMany(x => x.Items).OfType<IAssemblyReferenceExportItem>().ToList();
        }
    }
}
