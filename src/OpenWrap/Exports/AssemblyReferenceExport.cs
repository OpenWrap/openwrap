using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenWrap.Exports
{
    public class AssemblyReferenceExport : IExport
    {
        public AssemblyReferenceExport(IEnumerable<IExportItem> assemblies)
        {
            Items = assemblies.Select(x => CreateAssemblyRef(x)).Where(x => x != null).ToList();
        }

        public IEnumerable<IExportItem> Items { get; private set; }

        public string Name
        {
            get { return "bin"; }
        }

        static IExportItem CreateAssemblyRef(IExportItem item)
        {
            try
            {
                return new AssemblyReferenceExportItem(item);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}