using System.Collections.Generic;
using OpenWrap.Configuration;
using OpenWrap.Configuration.Core;
using OpenWrap.Services;

namespace OpenWrap.Commands.Core
{
    [Command(Verb = "get", Noun = "configuration", IsDefault = true)]
    public class GetConfigurationCommand : AbstractCommand
    {
        readonly IConfigurationManager _configurationManager;

        public GetConfigurationCommand() : this(ServiceLocator.GetService<IConfigurationManager>())
        {
        }

        public GetConfigurationCommand(IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
        }

        protected override IEnumerable<ICommandOutput> ExecuteCore()
        {
            var core = _configurationManager.Load<CoreConfiguration>();
            if (core == null)
            {
                yield return new EmptyConfiguration("core");
                yield break;
            }

            if (core.ProxyHref != null) yield return new ConfigurationData("proxy-href", core.ProxyHref);
            if (core.ProxyUsername != null) yield return new ConfigurationData("proxy-username", core.ProxyUsername);
            if (core.ProxyPassword != null) yield return new ConfigurationData("proxy-password", core.ProxyPassword);
        }
    }
}