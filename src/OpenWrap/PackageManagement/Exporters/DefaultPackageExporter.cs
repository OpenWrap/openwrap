using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;
using OpenWrap.Runtime;

namespace OpenWrap.PackageManagement.Exporters
{
    public class DefaultPackageExporter : IPackageExporter
    {
        readonly IEnumerable<IExportProvider> _providers;

        public DefaultPackageExporter(IEnumerable<IExportProvider> providers)
        {
            _providers = providers;
        }

        public void Initialize()
        {
        }

        public IEnumerable<IGrouping<string, TItems>> Exports<TItems>(IPackage package, ExecutionEnvironment environment = null) where TItems : IExportItem
        {
            return _providers.SelectMany(x => x.Items<TItems>(package, environment));
        }
    }
}