using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenWrap;
using OpenWrap.Build;
using OpenWrap.PackageModel;
using OpenWrap.PackageModel.Serialization;
using OpenWrap.Testing;

namespace Tests.Build.assembly_info
{
    public class context_assembly_info : context
    {
        const string ATTRIBUTE_TEXT = "[assembly: {0}(\"{1}\")]";
        protected IFile assembly_info_file;
        IPackageDescriptor descriptor;
        InMemoryFileSystem file_system;
        protected string assembly_info_content;

        public context_assembly_info()
        {
            file_system = new InMemoryFileSystem();
            assembly_info_file = file_system.GetTempDirectory().GetFile("GeneratedAssemblyinfo.cs");
        }
        protected void when_generating_assembly_info()
        {
            var generator = new AssemblyInfoGenerator(descriptor);
            generator.Write(assembly_info_file);
            assembly_info_content = generator.ToString();
        }

        protected void given_descriptor(params string[] lines)
        {
            descriptor = new PackageDescriptorReader()
                .Read(lines.JoinString("\r\n").ToUTF8Stream());
        }
        protected void should_have<T>(string value)
        {
            assembly_info_content.ShouldContain(
                string.Format(ATTRIBUTE_TEXT, typeof(T).FullName, value)
                );
        }
    }
}