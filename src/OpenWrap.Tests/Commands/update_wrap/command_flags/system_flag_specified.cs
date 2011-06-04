using NUnit.Framework;
using OpenWrap.Commands.contexts;
using OpenWrap.Commands.Wrap;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.update_wrap.command_flags
{
    public class system_flag_specified: contexts.update_wrap
    {
        UpdateWrapCommand CommandInstance;

        public system_flag_specified()
        {
            CommandInstance = new UpdateWrapCommand() { System = true };
        }
        [Test]
        public void project_is_selected()
        {
            CommandInstance.Project.ShouldBeFalse();
        }
        [Test]
        public void system_is_not_selected()
        {
            CommandInstance.System.ShouldBeTrue();
        }
    }
}