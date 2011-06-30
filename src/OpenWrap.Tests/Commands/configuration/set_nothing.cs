using NUnit.Framework;
using OpenWrap.Commands;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.configuration
{
    public class set_nothing : set_configuration
    {
        public set_nothing()
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