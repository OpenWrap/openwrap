using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenWrap;
using OpenWrap.IO;
using OpenWrap.PackageModel;
using OpenWrap.PackageModel.Serialization;
using OpenWrap.Tests.Commands;

namespace Tests.source_dir
{
    public class context_source_dir
    {
        InMemoryFileSystem file_system;
        protected ITemporaryDirectory root;
        protected InMemoryEnvironment env;
        protected IDirectory source;
        IPackageDescriptor descriptor;

        public context_source_dir()
        {
            file_system = new InMemoryFileSystem();
            root = file_system.CreateTempDirectory();
            env = new InMemoryEnvironment(root);
        }

        protected void given_folder(string folderName)
        {
            root.GetDirectory(folderName).MustExist();
        }

        protected void when_finding_source()
        {
            source = PathFinder.GetSourceDirectory(
                descriptor.DirectoryStructure,
                root
                );
        }

        protected void given_descriptor(params string[] line)
        {
            var descriptorText = line.JoinString("\r\n");
            descriptor = new PackageDescriptorReader().Read(descriptorText.ToUTF8Stream());
            root.GetFile("test.wrapdesc").WriteString(descriptorText);
        }
    }
}