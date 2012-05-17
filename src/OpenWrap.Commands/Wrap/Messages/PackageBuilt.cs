using OpenFileSystem.IO;

namespace OpenWrap.Commands.Wrap
{
    public class PackageBuilt : Info
    {
        public IFile File { get; set; }
        public SemanticVersion Version { get; set; }

        public PackageBuilt(IFile packageFilePath, SemanticVersion version)
            : base("Package built at '{0}'.", packageFilePath.Path)
        {
            File = packageFilePath;
            Version = version;
        }
    }
}