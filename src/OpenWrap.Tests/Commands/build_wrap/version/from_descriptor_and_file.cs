using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Commands.build_wrap.version
{
    public class from_descriptor_and_file : contexts.build_wrap
    {
        public from_descriptor_and_file()
        {
            given_descriptor(FileSystem.GetCurrentDirectory(), new PackageDescriptor()
            {
                Name = "test",
                Build = { "none" },
                SemanticVersion = "1.0.0.0".ToSemVer()
            });
            given_file("version", "2.0.0.0".ToUTF8Stream());
            given_current_directory_repository(new CurrentDirectoryRepository());
            when_executing_command();
        }

        [Test]
        public void build_successful()
        {
            Results.ShouldHaveNoError();
        }
        [Test]
        public void version_file_takes_precedence()
        {
            Environment.CurrentDirectoryRepository
                .PackagesByName["test"]
                .ShouldHaveCountOf(1).First().SemanticVersion.ShouldBe("2.0.0.0".ToSemVer());
        }
    }
}