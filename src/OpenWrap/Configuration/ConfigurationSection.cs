using System.Collections.Generic;

namespace OpenWrap.Configuration
{
    public class ConfigurationSection : ConfigurationEntry
    {
        public ConfigurationSection()
        {
            Lines = new List<ConfigurationLine>();
        }

        public ICollection<ConfigurationLine> Lines { get; private set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }
}