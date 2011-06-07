using NUnit.Framework;
using OpenWrap.Commands.Core;
using OpenWrap.Configuration.Core;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.configuration
{
    public class remove_proxy : configuration_command<RemoveConfigurationCommand>
    {
        public remove_proxy()
        {
            given_config(new CoreConfiguration { ProxyHref = "http://middle-earth/" });
            when_executing_command("-proxy");
        }

        [Test]
        public void message_notifies_of_update()
        {
            Results.ShouldHaveOne<ConfigurationUpdated>()
                .Types.ShouldContain("proxy-href");
        }

        [Test]
        public void proxy_is_cleared()
        {
            Configuration.ProxyHref.ShouldBeNull();
        }
    }
}