namespace OpenWrap.PackageModel.Parsers
{
    public class SingleBoolValue : SingleValue<bool>
    {
        public SingleBoolValue(PackageDescriptorEntryCollection entries, string name, bool defaultValue)
                : base(entries, name, x => x.ToString().ToLower(), bool.Parse, defaultValue: defaultValue)
        {
        }
        public static SingleBoolValue New(PackageDescriptorEntryCollection entries, string name, bool value)
        {
            return new SingleBoolValue(entries, name, value);
        }
    }
}