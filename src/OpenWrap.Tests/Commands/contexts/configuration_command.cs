using OpenWrap.Commands;
using OpenWrap.Configuration.Core;

namespace Tests.Commands.contexts
{
    public abstract class configuration_command<T> : command<T> where T : ICommand
    {

        protected CoreConfiguration Configuration;
        protected override void when_executing_command(string args = null)
        {
            base.when_executing_command(args);
            Configuration = ConfigurationManager.Load<CoreConfiguration>();
        }
    }
}