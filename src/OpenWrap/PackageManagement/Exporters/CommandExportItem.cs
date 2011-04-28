using OpenWrap.Commands;
using OpenWrap.PackageModel;

namespace OpenWrap.PackageManagement.Exporters
{
    public class CommandExportItem : Exports.ICommand
    {
        public CommandExportItem(string path, IPackage package, ICommandDescriptor descriptor)
        {
            Path = path;
            Package = package;
            Descriptor = descriptor;
        }

        public string Path { get; private set; }
        public IPackage Package { get; private set; }
        public ICommandDescriptor Descriptor { get; private set; }
    }
}