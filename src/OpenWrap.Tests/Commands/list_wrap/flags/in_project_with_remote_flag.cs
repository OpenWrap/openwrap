using NUnit.Framework;
using OpenWrap.Commands.Wrap;
using OpenWrap.Testing;

namespace Tests.Commands.list_wrap.flags
{
    public class in_project_with_remote_flag : command<ListWrapCommand>
    {
        public in_project_with_remote_flag()
        {
            given_project_repository();
            given_remote_repository("iron-hills");
            given_remote_repository("the-shire");
            when_executing_command("-remote");
        }
        [Test]
        public void project_is_ignored()
        {
            CommandInstance.Project.ShouldBeFalse();
        }
        [Test]
        public void system_is_ignored()
        {
            CommandInstance.System.ShouldBeFalse();
        }
        [Test]
        public void remotes_are_selected()
        {
            CommandInstance.SelectedRemotes
                .ShouldHaveCountOf(2)
                .ShouldHaveOne(_ => _.Name == "iron-hills")
                .ShouldHaveOne(_ => _.Name == "the-shire");

        }
    }
}