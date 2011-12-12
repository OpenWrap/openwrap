using NUnit.Framework;
using OpenWrap;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;

namespace Tests.Commands.build_wrap.providers.custom
{
    public class unknown_class : contexts.build_wrap
    {
        public unknown_class()
        {
            given_current_directory_repository(new CurrentDirectoryRepository());
            given_descriptor(FileSystem.GetCurrentDirectory(),
                             new PackageDescriptor
                             {
                                 Name = "test",
                                 Version = "1.0.0.0".ToSemVer(),
                                 Build = { "custom;typename=unknown,unknown" }
                             });
            when_executing_command();
        }

        [Test]
        public void error_is_generated()
        {
            Results.ShouldHaveError();
        }
    }
}