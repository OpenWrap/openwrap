using OpenWrap.Commands;

namespace OpenWrap.PackageManagement.Exporters
{
    public interface ICommandExportItem
    {
        ICommandDescriptor Descriptor { get; }
    }
}