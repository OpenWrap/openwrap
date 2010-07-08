using System.Collections.Generic;

namespace OpenWrap.Configuration
{
    public class ConfigurationSection : ConfigurationEntry
    {
        public ConfigurationSection()
        {
            Lines = new List<ConfigurationLine>();
        }
        public string Type { get; set; }
        public string Name { get; set; }
        public ICollection<ConfigurationLine> Lines { get; private set; }
    }
}