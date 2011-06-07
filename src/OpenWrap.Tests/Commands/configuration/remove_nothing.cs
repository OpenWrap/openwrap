using NUnit.Framework;
using OpenWrap.Commands;
using OpenWrap.Commands.Core;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.configuration
{
    public class remove_nothing : configuration_command<RemoveConfigurationCommand>
    {
        public remove_nothing()
        {
            when_executing_command();
        }

        [Test]
        public void error_is_returned()
        {
            Results.ShouldHaveOne<Error>();
        }
    }
}