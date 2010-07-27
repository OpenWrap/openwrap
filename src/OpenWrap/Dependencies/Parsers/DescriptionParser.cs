using System;

namespace OpenWrap.Dependencies
{
    public class DescriptionParser : AbstractDescriptorParser
    {
        public DescriptionParser() : base("description")
        {
        }
        public override void ParseContent(string content, WrapDescriptor descriptor)
        {
            descriptor.Description = content.Trim();
        }
    }
}