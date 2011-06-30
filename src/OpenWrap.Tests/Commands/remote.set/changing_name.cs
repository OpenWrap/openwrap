using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Commands.remote.set
{
    public class changing_name : contexts.set_remote
    {
        public changing_name()
        {
            given_remote_config("secundus");
            when_executing_command("secundus -newname vamu");
        }

        [Test]
        public void repository_with_new_name_exists()
        {
            var remote = TryGetRepository("vamu");
            remote.Name.ShouldBe("vamu");
        }

        [Test]
        public void repository_with_old_name_removed()
        {
            ConfiguredRemotes.ContainsKey("secundus").ShouldBeFalse();
        }
    }
}