using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands.Core;
using OpenWrap.Configuration.Core;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.configuration
{
    public class get_without_arguments_with_config : configuration_command<GetConfigurationCommand>
    {
        public get_without_arguments_with_config()
        {
            given_config(new CoreConfiguration { ProxyHref = "http://middle-earth/", ProxyUsername = "sauron", ProxyPassword = "froddo" });
            when_executing_command();
        }

        [Test]
        public void set_config_is_returned()
        {
            Results.OfType<ConfigurationData>()
                .ShouldHaveOne(x => x.Name == "proxy-href" && x.Value == "http://middle-earth/")
                .ShouldHaveOne(x => x.Name == "proxy-username" && x.Value == "sauron").ShouldHaveOne(x => x.Name == "proxy-password" && x.Value == "froddo");
        }
    }
}