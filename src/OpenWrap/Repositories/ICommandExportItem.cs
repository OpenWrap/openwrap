using OpenWrap.Commands;

namespace OpenWrap.Repositories
{
    public interface ICommandExportItem
    {
        ICommandDescriptor Descriptor { get; }
    }
}