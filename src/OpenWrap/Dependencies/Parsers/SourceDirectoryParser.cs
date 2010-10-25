using System.Collections.Generic;

namespace OpenWrap.Dependencies.Parsers
{
    public class SourceDirectoryParser : AbstractDescriptorParser
    {
        public SourceDirectoryParser() : base("source-directory") { }

        protected override void ParseContent(string content, PackageDescriptor descriptor)
        {
            string source = content.Trim();
            descriptor.SourceDirectory = string.IsNullOrEmpty(source) ? null : source;
        }
        protected override IEnumerable<string> WriteContent(PackageDescriptor descriptor)
        {
            if (!string.IsNullOrEmpty(descriptor.SourceDirectory))
                yield return descriptor.SourceDirectory;
        }
    }
}