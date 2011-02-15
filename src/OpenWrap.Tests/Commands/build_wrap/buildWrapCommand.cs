using System;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenWrap.Commands.contexts;
using OpenWrap.Commands.Wrap;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace OpenWrap.Tests.Commands.Wrap
{
    public class building_a_meta_package : command_context<BuildWrapCommand>
    {
        static Version version = new Version("1.2.3.4");
        const string packageName = "mypackage";
        public building_a_meta_package()
        {
            given_current_directory_repository(new CurrentDirectoryRepository());
            Environment.Descriptor.Name = packageName;
            Environment.Descriptor.Version = version;
            Environment.Descriptor.Build.Add("none");
            Environment.Descriptor.Version = version;
            
            when_executing_command();
        }

        [Test]
        public void wrapdescriptor_included_in_package()
        {
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
