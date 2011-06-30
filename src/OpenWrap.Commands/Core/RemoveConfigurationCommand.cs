using System.Collections.Generic;
using OpenWrap.Configuration;
using OpenWrap.Configuration.Core;
using OpenWrap.Services;

namespace OpenWrap.Commands.Core
{
    [Command(Verb = "remove", Noun = "configuration")]
    public class RemoveConfigurationCommand : AbstractCommand
    {
        readonly IConfigurationManager _configurationManager;

        public RemoveConfigurationCommand() : this(ServiceLocator.GetService<IConfigurationManager>())
        {
        }

        public RemoveConfigurationCommand(IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
        }

        [CommandInput]
        public bool Proxy { get; set; }

        protected override IEnumerable<ICommandOutput> ExecuteCore()
        {
            var core = _configurationManager.Load<CoreConfiguration>() ?? new CoreConfiguration();
            var updates = new List<string>();

            if (Proxy)
            {
                core.ProxyHref = core.ProxyUsername = core.ProxyPassword = null;
                updates.AddRange(new[] { "proxy-href", "proxy-username", "proxy-password" });
            }
            if (updates.Count == 0)
            {
                yield return new Error("No configuration has been provided.");
                yield break;
            }

            _configurationManager.Save(core);
            yield return new ConfigurationUpdated(updates);
        }
    }
}