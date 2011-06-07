using NUnit.Framework;
using OpenWrap.Commands.Wrap;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.update_wrap.command_flags
{
    public class defaults: contexts.update_wrap
    {
        UpdateWrapCommand CommandInstance;

        public defaults()
        {
            CommandInstance = new UpdateWrapCommand();
        }
        [Test]public void project_is_selected()
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