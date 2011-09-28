using OpenFileSystem.IO;

namespace OpenWrap.Commands.Wrap
{
    public class PackageBuilt : Info
    {
        public IFile PackageFile { get; set; }

        public PackageBuilt(IFile packageFilePath)
            : base("Package built at '{0}'.", packageFilePath.Path)
        {
            PackageFile = packageFilePath;
        }
    }
}