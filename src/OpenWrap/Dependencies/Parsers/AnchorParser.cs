using System;
using System.Text.RegularExpressions;

namespace OpenWrap.Dependencies
{
    public class AnchorParser : AbstractDescriptorParser
    {
        public AnchorParser() : base("anchored"){}

        public override void ParseContent(string content, WrapDescriptor descriptor)
        {
            descriptor.IsAnchored = string.Compare(content.Trim(), "true", StringComparison.OrdinalIgnoreCase) == 0;
        }
    }
}