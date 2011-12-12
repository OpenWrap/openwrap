using System;

namespace OpenWrap.PackageModel.Parsers
{
    public class SingleVersionValue : SingleValue<SemanticVersion>
    {
        public SingleVersionValue(PackageDescriptorEntryCollection entries, string name)
                : base(
            entries,
            name, 
            x => x != null ? x.ToString() : null, 
            SemanticVersion.TryParseExact)
        {
        }
        public static SingleVersionValue New(PackageDescriptorEntryCollection entries, string name, SemanticVersion defaultVal)
        {
            return new SingleVersionValue(entries, name);
        }

    }
}