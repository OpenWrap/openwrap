using System;
using OpenWrap.Commands;
using OpenWrap.Exports;

namespace OpenWrap.Repositories
{
    public class CommandExportItem : IExportItem, ICommandExportItem
    {
        public ICommandDescriptor Descriptor { get; private set; }
        public CommandExportItem(Type commandTypes)
        {
            Descriptor = new AttributeBasedCommandDescriptor(commandTypes);
            FullPath = commandTypes.Assembly.Location;
        }

        public string FullPath
        {
            get; private set;
        }
    }
}