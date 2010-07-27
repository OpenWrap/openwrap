using System;

namespace OpenWrap.Dependencies
{
    public class OverrideParser : AbstractDescriptorParser
    {
        public OverrideParser() : base("override")
        {
            
        }
        public override void ParseContent(string content, WrapDescriptor descriptor)
        {

            var arguments = content.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // TODO: Should this throw, or just return?
            // Perhaps we need a way to record parse errors for better error reporting?
            if (arguments.Length != 2) throw new Exception("Parse error in \"override\" declaration. Expected format: \"override {old-package} {new-package}.\". Actual line: \"" + content + "\"");

            var oldPackage = arguments[0];
            var newPackage = arguments[1];
            descriptor.Overrides.Add(new WrapOverride(oldPackage, newPackage));
        }
    }
}
