using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Exports;

namespace OpenWrap.Repositories
{
    internal class CommandExport : IExport
    {
        public CommandExport(IEnumerable<Type> commandTypes)
        {
            Items = commandTypes.Select(x => (IExportItem)new CommandExportItem(x)).ToList();
        }

        public string Name
        {
            get { return "Commands"; }
        }

        public IEnumerable<IExportItem> Items
        {
            get; private set;
        }
    }
}