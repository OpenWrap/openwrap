using NUnit.Framework;
using OpenWrap.Commands.Wrap;
using OpenWrap.Testing;

namespace Tests.Commands.list_wrap.flags
{
    public class in_project_with_system_flag : command<ListWrapCommand>
    {
        public in_project_with_system_flag()
        {
            given_project_repository();
            when_executing_command("-system");
        }
        [Test]
        public void project_is_ignored()
        {
            CommandInstance.Project.ShouldBeFalse();
        }
        [Test]
        public void system_is_selected()
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