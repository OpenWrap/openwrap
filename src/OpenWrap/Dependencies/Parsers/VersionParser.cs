using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenWrap.Dependencies
{
    public class VersionParser : AbstractDescriptorParser
    {
        public VersionParser() : base("version")
        {
        }

        protected override void ParseContent(string content, WrapDescriptor descriptor)
        {
            descriptor.Version = new Version(content);
            descriptor.IsVersionInDescriptor = true;
        }
        public override string GetContentRegex()
        {
            return @"\d+(\.\d+(\.\d+(\.\d+)?)?)?";
        }
        protected override IEnumerable<string> WriteContent(WrapDescriptor descriptor)
        {
            if (descriptor.IsVersionInDescriptor)
                yield return descriptor.Version.ToString();
        }
    }
}
