namespace OpenWrap.Dependencies
{
    public class SingleStringValue : SingleValue<string>
    {
        public SingleStringValue(DescriptorLineCollection lines, string name,  string defaultValue = null)
                : base(lines, name, x => x, x => x, defaultValue: defaultValue) { }

    }
}