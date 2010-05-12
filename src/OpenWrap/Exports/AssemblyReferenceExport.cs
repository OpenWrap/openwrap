using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Wrap.Resources;

namespace OpenRasta.Wrap.Repositories
{
    public class AssemblyReferenceExport : IWrapExport
    {
        public AssemblyReferenceExport(IEnumerable<IWrapExportItem> assemblies)
        {
            Items = assemblies.Select(x => CreateAssemblyRef(x)).Where(x => x != null).ToList();
        }

        public IEnumerable<IWrapExportItem> Items { get; private set; }

        public string Name
        {
            get { return "bin"; }
        }

        static IWrapExportItem CreateAssemblyRef(IWrapExportItem item)
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