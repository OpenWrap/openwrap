using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Commands.build_wrap.version
{
    public class from_descriptor : contexts.build_wrap
    {
        public from_descriptor()
        {
            given_descriptor(FileSystem.GetCurrentDirectory(), new PackageDescriptor()
            {
                Name = "test", Version="1.0.0.0".ToVersion(), Build = {"none"}
            });
            
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
            Enumerable.First<IPackageInfo>(this.
                                         Environment.CurrentDirectoryRepository
                                         .PackagesByName["test"]
                                         .ShouldHaveCountOf(1)).Version.ShouldBe("1.0.0.0".ToVersion());
        }
    }
}