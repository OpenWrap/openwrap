using System.Reflection;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenWrap;
using OpenWrap.Build;
using OpenWrap.PackageModel;
using OpenWrap.PackageModel.Serialization;
using OpenWrap.Testing;

namespace Tests.Build.assembly_info
{
    public class none :context_assembly_info
    {
        public none()
        {
            given_descriptor();
            when_generating_assembly_info();
        }

        [Test]
        public void no_file_generated()
        {
            assembly_info_file.Exists.ShouldBeFalse();
        }
    }
    public class copyright : context_assembly_info
    {
        public copyright()
        {
            given_descriptor(
                "copyright: Tolkien",
                "assembly-info: copyright");
            when_generating_assembly_info();
        }
        [Test]
        public void contains_attribute()
        {
            should_have<AssemblyCopyrightAttribute>("Tolkien");
        }
    }

    public class assembly_file_version : context_assembly_info
    {
        public assembly_file_version()
        {
            given_descriptor(
                "version: 1.0.0." + ushort.MaxValue,
                "assembly-info: file-version");
            when_generating_assembly_info();
        }
        [Test]
        public void contains_attribute()
        {
            should_have<AssemblyFileVersionAttribute>(
                "1.0.0.65535");
        }
    }
    public class assembly_version : context_assembly_info
    {
        public assembly_version()
        {
            given_descriptor(
                "version: 1.0.0." + ushort.MaxValue,
                "assembly-info: assembly-version");
            when_generating_assembly_info();
        }
        [Test]
        public void contains_attribute()
        {
            should_have<AssemblyVersionAttribute>(
                "1.0.0.0");
        }
    }
    public class author : context_assembly_info
    {
        public author()
        {
            given_descriptor(
                "author: sauron <sauron@middle.earth>",
                "author: frodo <frodo@middle.earth>",
                "assembly-info: author");
            when_generating_assembly_info();
        }
        [Test]
        public void contains_attribute()
        {
            should_have<AssemblyCompanyAttribute>(
                "sauron <sauron@middle.earth>, frodo <frodo@middle.earth>");
        }
    }
    public class title : context_assembly_info
    {
        public title()
        {
            given_descriptor(
                "title: The lord of the rings",
                "assembly-info: title");
            when_generating_assembly_info();
        }
        [Test]
        public void contains_attribute()
        {
            should_have<AssemblyProductAttribute>("The lord of the rings");
        }
    }

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