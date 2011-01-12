using System;

namespace OpenWrap.PackageModel.Parsers
{
    public class SingleVersionValue : SingleValue<Version>
    {
        public SingleVersionValue(PackageDescriptorEntryCollection entries, string name)
                : base(entries, name, x => x != null ? x.ToString() : null, x => x.ToVersion())
        {
        }
        public static SingleVersionValue New(PackageDescriptorEntryCollection entries, string name, Version defaultVal)
        {
            return new SingleVersionValue(entries, name);
        }

    }
}