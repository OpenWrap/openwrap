using System;

namespace OpenWrap.PackageModel.Parsers
{
    public class PackageNameOverrideCollection : MultiLine<PackageNameOverride>
    {
        public PackageNameOverrideCollection(PackageDescriptorEntryCollection entries)
                : base(entries, "override", ConvertToString, ConvertFromString)
        {
        }

        static PackageNameOverride ConvertFromString(string arg)
        {
            var names = arg.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (names.Length == 2)
                return new PackageNameOverride(names[0].Trim(), names[1].Trim());
            return null;
        }

        static string ConvertToString(PackageNameOverride arg)
        {
            return string.Format("{0} {1}", arg.OldPackage, arg.NewPackage);
        }
    }
}