using NUnit.Framework;
using OpenWrap.Commands.Wrap;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.update_wrap.command_flags
{
    public class project_flag_specified: contexts.update_wrap
    {
        UpdateWrapCommand CommandInstance;

        public project_flag_specified()
        {
            CommandInstance = new UpdateWrapCommand() { Project = true};
        }
        [Test]
        public void project_is_selected()
        {
            CommandInstance.Project.ShouldBeTrue();
        }
        [Test]
        public void system_is_not_selected()
        {
            CommandInstance.System.ShouldBeFalse();
        }
    }
}