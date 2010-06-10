using System;

namespace OpenWrap.Dependencies
{
    public class WrapDescriptionParser : IWrapDescriptorLineParser
    {
        public void Parse(string line, WrapDescriptor descriptor)
        {
            if (!line.StartsWith("description", StringComparison.OrdinalIgnoreCase)) return;

            var textStart = "description".Length;
            descriptor.Description = line.Substring(textStart).Trim();
        }
    }
}