using System;
using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands.Wrap;
using OpenWrap.Repositories;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.build_wrap
{
    public class building_a_meta_package : command<BuildWrapCommand>
    {
        public building_a_meta_package()
        {
            given_current_directory_repository(new CurrentDirectoryRepository());
            given_descriptor(
                "name: one-ring",
                "version: 1.2.3.4",
                "build: none"
                );
            when_executing_command();
        }


        [Test]
        public void is_packaged()
        {
            Environment.CurrentDirectoryRepository.ShouldHavePackage("one-ring", "1.2.3.4");
        }

        [Test]
        public void package_information_is_output()
        {
            Results.OfType<PackageBuilt>().Single().ShouldNotBeNull().PackageFile.Exists.ShouldBeTrue();
        }
    }
}
