using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Commands.build_wrap.version
{
    public class from_version_file : contexts.build_wrap
    {
        public from_version_file()
        {
            given_descriptor("name: test", "build: none");
            given_file("version", "1.0.0.0".ToUTF8Stream());
            given_current_directory_repository(new CurrentDirectoryRepository());
            when_executing_command();
        }

        [Test]
        public void build_successful()
        {
            Results.ShouldHaveNoError();
        }

        [Test]
        public void package_has_correct_version()
        {
            Environment.CurrentDirectoryRepository
                .PackagesByName["test"]
                .ShouldHaveOne().Version.ShouldBe("1.0.0.0".ToSemVer());
        }
    }
}