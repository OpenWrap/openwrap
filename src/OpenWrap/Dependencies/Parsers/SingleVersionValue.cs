using System;

namespace OpenWrap.Dependencies
{
    public class SingleVersionValue : SingleValue<Version>
    {
        public SingleVersionValue(DescriptorLineCollection lines, string name)
                : base(lines, name, x => x != null ? x.ToString() : null, x => x.ToVersion())
        {

        }
    }
}