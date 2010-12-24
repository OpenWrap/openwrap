namespace OpenWrap.PackageModel.Parsers
{
    public class SingleBoolValue : SingleValue<bool>
    {
        public SingleBoolValue(PackageDescriptorEntryCollection entries, string name, bool defaultValue)
                : base(entries, name, x => x.ToString().ToLower(), bool.Parse, defaultValue: defaultValue)
        {
        }
    }
}