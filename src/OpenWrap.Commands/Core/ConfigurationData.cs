namespace OpenWrap.Commands.Core
{
    public class ConfigurationData : Info
    {
        public ConfigurationData(string name, string value)
            : base("{0:-15}: {1}", name, value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }
        public string Value { get; set; }
    }
}