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
        public override void ParseContent(string content, WrapDescriptor descriptor)
        {
            descriptor.Version = new Version(content);
        }
        public override string GetContentRegex()
        {
            return @"\d+(\.\d+(\.\d+(\.\d+)?)?)?";
        }
    }
}
