using System;

namespace OpenWrap.Dependencies
{
    public class PackageNameOverrideCollection : MultiLine<PackageNameOverride>
    {
        public PackageNameOverrideCollection(DescriptorLineCollection lines)
                : base(lines, "override", ConvertToString, ConvertFromString)
        {
        }

        static string ConvertToString(PackageNameOverride arg)
        {
            return string.Format("{0} {1}", arg.OldPackage, arg.NewPackage);
        }

        static PackageNameOverride ConvertFromString(string arg)
        {
            var names = arg.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (names.Length == 2)
                return new PackageNameOverride(names[0].Trim(), names[1].Trim());
            return null;
        }
    }
}