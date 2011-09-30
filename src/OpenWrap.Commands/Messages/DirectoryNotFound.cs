using OpenFileSystem.IO;

namespace OpenWrap.Commands.Messages
{
    public class DirectoryNotFound : Error
    {
        public IDirectory Directory { get; set; }

        public DirectoryNotFound(IDirectory directory)
            : base("'{0}' does not exist or is not a directory.", directory.Path)
        {
            Directory = directory;
        }
    }
    public class PackageDescriptorNotFound : Error
    {
        public IDirectory Directory { get; set; }

        public PackageDescriptorNotFound(IDirectory directory)
            : base("'{0}' does not contain any package descriptors. Make sure the path is correct and there is a .wrapdesc file.", directory.Path)
        {
            Directory = directory;
        }
    }
}