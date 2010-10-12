using System;
using System.Collections.Generic;

namespace OpenWrap.Dependencies.Parsers
{
    public class AnchorParser : AbstractDescriptorParser
    {
        public AnchorParser() : base("anchored"){}

        protected override void ParseContent(string content, PackageDescriptor descriptor)
        {
            descriptor.Anchored = string.Compare(content.Trim(), "true", StringComparison.OrdinalIgnoreCase) == 0;
        }
        protected override IEnumerable<string> WriteContent(PackageDescriptor descriptor)
        {
            if (descriptor.Anchored)
                yield return "true";
        }
    }
}