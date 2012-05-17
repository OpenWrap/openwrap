using System.Linq;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenWrap;
using OpenWrap.PackageModel;
using OpenWrap.PackageModel.Serialization;
using OpenWrap.Testing;

namespace Tests.Commands.init_wrap
{
    class init_dot_in_existing_package : contexts.init_wrap
    {
        public init_dot_in_existing_package()
        {
            given_current_directory(@"c:\newpackage");
            given_dependency("depends: openwrap");
            given_dependency_file("newpackage.wrapdesc");
            given_project_package("openwrap", "1.0.0");
            when_executing_command(".");
        }

        [Test]
        public void dependency_is_not_updated()
        {
            new PackageDescriptorReader().Read(Environment.CurrentDirectory.GetFile("newpackage.wrapdesc"))
                    .Dependencies.SingleOrDefault(x => x.Name.EqualsNoCase("openwrap"))
                    .ContentOnly.ShouldBeFalse();
        }

        void given_dependency_file(string name)
        {
            new PackageDescriptorWriter()
                    .Write(
                            Environment.Descriptor,
                            Environment.CurrentDirectory.GetFile(name).OpenWrite());
        }
    }
}