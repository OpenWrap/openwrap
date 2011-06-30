using System;
using System.Collections.Generic;
using OpenRasta.Client;
using OpenWrap.Configuration;
using OpenWrap.Configuration.Core;
using OpenWrap.Services;

namespace OpenWrap.Commands.Core
{
    [Command(Verb = "set", Noun = "configuration")]
    public class SetConfigurationCommand : AbstractCommand
    {
        readonly IConfigurationManager _configurationManager;

        public SetConfigurationCommand() : this(ServiceLocator.GetService<IConfigurationManager>())
        {
        }

        public SetConfigurationCommand(IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
        }

        [CommandInput]
        public string Proxy { get; set; }

        protected override IEnumerable<ICommandOutput> ExecuteCore()
        {
            var core = _configurationManager.Load<CoreConfiguration>() ?? new CoreConfiguration();
            var changes = new List<string>();
            if (Proxy != null)
            {
                var proxyUri = Proxy.ToUri();
                if (proxyUri == null || !proxyUri.IsAbsoluteUri)
                {
                    yield return new InvalidProxy(Proxy);
                    yield break;
                }

                if (!string.IsNullOrEmpty(proxyUri.UserInfo))
                {
                    var builder = new UriBuilder(proxyUri);
                    changes.AddRange(SetUsernamePassword(core, builder));

                    builder.UserName = string.Empty;
                    builder.Password = string.Empty;
                    proxyUri = builder.Uri;
                }
                core.ProxyHref = proxyUri.ToString();
                changes.Add("proxy-href");
            }
            if (changes.Count == 0)
            {
                yield return new Error("No configuration has been provided.");
                yield break;
            }


            _configurationManager.Save(core);
            yield return new ConfigurationUpdated(changes);
        }

        static IEnumerable<string> SetUsernamePassword(CoreConfiguration core, UriBuilder builder)
        {
            core.ProxyUsername = null;
            core.ProxyPassword = null;

            if (!string.IsNullOrEmpty(builder.UserName))
            {
                core.ProxyUsername = builder.UserName;
                yield return "proxy-username";
            }

            if (!string.IsNullOrEmpty(builder.Password))
            {
                core.ProxyPassword = Uri.UnescapeDataString(builder.Password);
                yield return "proxy-password";
            }
        }
    }
}