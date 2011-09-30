using NUnit.Framework;
using OpenWrap.Commands;
using OpenWrap.Commands.Messages;
using OpenWrap.Commands.Wrap;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Commands.build_wrap
{
    public class no_descriptor :  command<BuildWrapCommand>
    {
        public no_descriptor()
        {
            given_current_directory_repository(new CurrentDirectoryRepository());
            
            when_executing_command();
        }

        [Test]
        public void error_is_displayed()
        {
            Results.ShouldHaveOne<PackageDescriptorNotFound>()
                .Directory.ShouldBe(FileSystem.GetCurrentDirectory());
        }
    }
}