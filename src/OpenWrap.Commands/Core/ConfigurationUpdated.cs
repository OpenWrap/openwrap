using System.Collections.Generic;
using System.Linq;

namespace OpenWrap.Commands.Core
{
    public class ConfigurationUpdated : Info
    {
        public ConfigurationUpdated(IEnumerable<string> types)
            : base("Configuration for {0} updated.", types.Select(_ => "'" + _ + "'").JoinString(", "))
        {
            Types = types;
        }

        public IEnumerable<string> Types { get; set; }
    }
}