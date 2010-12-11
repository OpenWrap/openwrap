using System;
using System.Collections.Generic;

namespace OpenWrap.Dependencies
{
    public class VersionParser : AbstractDescriptorParser
    {
        public VersionParser()
            : base("version")
        {
        }

        protected override void ParseContent(string content, PackageDescriptor descriptor)
        {
            descriptor.Version = new Version(content);
        }
        public virtual string GetContentRegex()
        {
            return @"\d+(\.\d+(\.\d+(\.\d+)?)?)?";
        }
        protected override IEnumerable<string> WriteContent(PackageDescriptor descriptor)
        {
            if (descriptor.Version != null)
                yield return descriptor.Version.ToString();
        }
    }
}
