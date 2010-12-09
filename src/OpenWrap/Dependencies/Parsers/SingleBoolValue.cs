namespace OpenWrap.Dependencies
{
    public class SingleBoolValue : SingleValue<bool>
    {
        public SingleBoolValue(DescriptorLineCollection lines, string name, bool defaultValue)
                : base(lines, name, x => x.ToString().ToLower(), bool.Parse, defaultValue: defaultValue)
        { }
    }
}