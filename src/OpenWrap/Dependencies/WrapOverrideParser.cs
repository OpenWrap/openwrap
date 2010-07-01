using System;

namespace OpenWrap.Dependencies
{
    public class WrapOverrideParser : IWrapDescriptorLineParser
    {
        public void Parse(string line, WrapDescriptor descriptor)
        {
            if (!line.StartsWith("override", StringComparison.OrdinalIgnoreCase)) return;

            var textStart = "override".Length;
            var arguments = line.Substring(textStart).Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // TODO: Should this throw, or just return?
            // Perhaps we need a way to record parse errors for better error reporting?
            if (arguments.Length != 2) throw new Exception("Parse error in \"override\" declaration. Expected format: \"override {old-package} {new-package}.\". Actual line: \"" + line + "\"");
            
            var oldPackage = arguments[0];
            var newPackage = arguments[1];
            descriptor.Overrides.Add(new WrapOverride(oldPackage, newPackage));
        }

    }
}
