using NUnit.Framework;
using OpenWrap.Commands.Wrap;
using OpenWrap.Testing;

namespace Tests.Commands.list_wrap.flags
{
    public class in_project_defaults : command<ListWrapCommand>
    {
        public in_project_defaults()
        {
            given_project_package("one-ring", "1.0");
            when_executing_command();
        }
        [Test]
        public void project_is_default()
        {
            CommandInstance.Project.ShouldBeTrue();
        }
        [Test]
        public void system_is_ignored()
        {
            CommandInstance.System.ShouldBeFalse();
        }
        [Test]
        public void remotes_are_ignored()
        {
            CommandInstance.SelectedRemotes.ShouldBeEmpty();
        }
    }
}