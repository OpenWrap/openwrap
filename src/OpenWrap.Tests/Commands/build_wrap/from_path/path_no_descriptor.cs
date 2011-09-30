using NUnit.Framework;
using OpenWrap.Commands.Messages;
using OpenWrap.Testing;

namespace Tests.Commands.build_wrap.from_path
{
    public class path_no_descriptor : from_remote_path
    {
        public path_no_descriptor()
        {
            given_remote_project();
            when_executing_command("-from " + path_to_project.Path);
        }

        [Test]
        public void error_is_displayed()
        {
            Results.ShouldHaveOne<PackageDescriptorNotFound>()
                .Directory.ShouldBe(path_to_project);
        }
    }
}