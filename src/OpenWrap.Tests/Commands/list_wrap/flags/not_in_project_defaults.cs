using NUnit.Framework;
using OpenWrap.Commands.Wrap;
using OpenWrap.Testing;

namespace Tests.Commands.list_wrap.flags
{
    public class not_in_project_defaults : command<ListWrapCommand>
    {
        public not_in_project_defaults()
        {
            when_executing_command();
        }
        [Test]
        public void project_is_ignored()
        {
            CommandInstance.Project.ShouldBeFalse();
        }
        [Test]
        public void system_is_default()
        {
            CommandInstance.System.ShouldBeTrue();
        }
        [Test]
        public void remotes_are_ignored()
        {
            CommandInstance.SelectedRemotes.ShouldBeEmpty();
        }
    }
}