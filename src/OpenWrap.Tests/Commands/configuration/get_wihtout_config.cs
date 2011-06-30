using NUnit.Framework;
using OpenWrap.Commands.Core;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.configuration
{
    public class get_wihtout_config : configuration_command<GetConfigurationCommand>
    {
        public get_wihtout_config()
        {
            when_executing_command();
        }

        [Test]
        public void empty_config_message_issued()
        {
            Results.ShouldHaveOne<EmptyConfiguration>();
        }
    }
}