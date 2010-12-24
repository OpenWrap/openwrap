using System;
using OpenWrap.Commands;

namespace OpenWrap.PackageManagement.Exporters
{
    public class CommandExportItem : IExportItem, ICommandExportItem
    {
        public CommandExportItem(Type commandTypes)
        {
            Descriptor = new AttributeBasedCommandDescriptor(commandTypes);
            FullPath = commandTypes.Assembly.Location;
        }

        public ICommandDescriptor Descriptor { get; private set; }

        public string FullPath { get; private set; }
    }
}