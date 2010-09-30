using System;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenWrap.Commands.Wrap;
using OpenWrap.Testing;

namespace OpenWrap.Tests.Commands.Wrap
{
    public class building_a_meta_package : context.command_context<BuildWrapCommand>
    {
        static Version version = new Version("1.2.3.4");
        const string packageName = "mypackage";
        public building_a_meta_package()
        {
            Environment.Descriptor.BuildCommand = "$meta";
            given_file(packageName + ".wrapdesc", new MemoryStream(Encoding.Default.GetBytes("build: $meta")));
            given_file("version", new MemoryStream(Encoding.Default.GetBytes(version.ToString())));
            
            when_executing_command();
        }

        [Test]
        public void wrapdescriptor_included_in_package()
        {
            //Environment.CurrentDirectoryRepository.
        }

        [Test]
        public void wrap_exists()
        {
            Environment.CurrentDirectoryRepository
                    .PackagesByName[packageName]
                    .ShouldHaveCountOf(1);
        }

        [Test]
        public void wrap_has_correct_version_number()
        {
            Environment.CurrentDirectoryRepository
                    .PackagesByName[packageName]
                    .ShouldHaveCountOf(1)
                    .First().Version.ShouldBe(version);

        }
    }
}
