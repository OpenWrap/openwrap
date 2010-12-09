namespace OpenWrap.Dependencies
{
    public class PackageDependencyCollection : MultiLine<PackageDependency>
    {

        public PackageDependencyCollection(DescriptorLineCollection lines)
                : base(lines, "depends", ConvertToString, ConvertFromString)
        {
        }

        static string ConvertToString(PackageDependency arg)
        {
            return arg.ToString();
        }

        static PackageDependency ConvertFromString(string arg)
        {
            return DependsParser.ParseDependsValue(arg);
        }
    }
}