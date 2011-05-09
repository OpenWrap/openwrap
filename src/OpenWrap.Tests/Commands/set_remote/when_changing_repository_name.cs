using NUnit.Framework;
using OpenWrap.Testing;
using OpenWrap.Tests.Commands.Remote.Set.context;

namespace OpenWrap.Tests.Commands.Remote.Set
{
    public class when_changing_repository_name : set_remote
    {
        public when_changing_repository_name()
        {
            when_executing_command("secundus -newname vamu");
        }

        [Test]
        public void the_second_repository_has_new_name()
        {
            var remote = TryGetRepository("secundus");
            remote.Name.ShouldBe("vamu");
        }
    }
}