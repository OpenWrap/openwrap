using System;
using System.Linq;

namespace OpenWrap.Dependencies
{
    public class OverrideParser : AbstractDescriptorParser
    {
        public OverrideParser() : base("override")
        {
            
        }

        protected override void ParseContent(string content, WrapDescriptor descriptor)
        {

            var arguments = content.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (arguments.Length != 2) throw new InvalidOperationException("Parse error in override declaration. Expected format: \"override: {old-package} {new-package}.\". Actual line: \"" + content + "\"");

            var oldPackage = arguments[0];
            var newPackage = arguments[1];
            descriptor.Overrides.Add(new WrapOverride(oldPackage, newPackage));
        }
        protected override System.Collections.Generic.IEnumerable<string> WriteContent(WrapDescriptor descriptor)
        {
            return descriptor.Overrides.Select(x => x.OldPackage + " " + x.NewPackage);
        }
    }
}
