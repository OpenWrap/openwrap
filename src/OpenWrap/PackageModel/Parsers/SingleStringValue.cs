namespace OpenWrap.PackageModel.Parsers
{
    public class SingleStringValue : SingleValue<string>
    {
        public SingleStringValue(PackageDescriptorEntryCollection entries, string name, string defaultValue = null)
                : base(entries, name, x => x, x => x, defaultValue: defaultValue)
        {
        }
        public static SingleStringValue New(PackageDescriptorEntryCollection entries, string name, string defaultVal)
        {
            return new SingleStringValue(entries, name, defaultVal);
        }

    }
}