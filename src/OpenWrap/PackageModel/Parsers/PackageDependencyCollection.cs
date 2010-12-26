using System.Collections.Generic;

namespace OpenWrap.PackageModel.Parsers
{
    public class PackageDependencyCollection : MultiLine<PackageDependency>
    {
        public PackageDependencyCollection(PackageDescriptorEntryCollection entries)
                : base(entries, "depends", ConvertToString, ConvertFromString)
        {
        }

        static PackageDependency ConvertFromString(string arg)
        {
            return DependsParser.ParseDependsValue(arg);
        }

        static string ConvertToString(PackageDependency arg)
        {
            return arg.ToString();
        }
    }
}