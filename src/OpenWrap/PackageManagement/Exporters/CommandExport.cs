using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenWrap.PackageManagement.Exporters
{
    internal class CommandExport : IExport
    {
        public CommandExport(IEnumerable<Type> commandTypes)
        {
            Items = commandTypes.Select(x => (IExportItem)new CommandExportItem(x)).ToList();
        }

        public IEnumerable<IExportItem> Items { get; private set; }

        public string Name
        {
            get { return "Commands"; }
        }
    }
}