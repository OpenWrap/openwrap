using System;

namespace OpenWrap.PackageModel.Parsers
{
    public class SingleVersionValue : SingleValue<Version>
    {
        public SingleVersionValue(PackageDescriptorEntryCollection entries, string name)
                : base(entries, name, x => x != null ? x.ToString() : null, x => x.ToVersion())
        {
        }
    }
}