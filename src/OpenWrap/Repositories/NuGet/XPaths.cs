namespace OpenWrap.Repositories.NuGet
{
    public static class XPaths
    {
        static XPaths()
        {
            Package = "//" + Local("package") + "/";
            Metadata = Package + Local("metadata") + "/";
            Dependencies = Metadata + Local("dependencies") + "/" + Local("dependency");
            OldDependencies = Package + Local("dependencies") + "/" + Local("dependency");
            PackageDependencies = new[] { Dependencies, OldDependencies };
            PackageDescription = new[] { Metadata + Local("description") };

            PackageName = new[] { Metadata + Local("id") };
            PackageVersion = new[] { Metadata + Local("version") };
        }

        public static readonly string Dependencies;
        public static readonly string Metadata;
        public static readonly string OldDependencies;
        public static readonly string Package;
        public static readonly string[] PackageDependencies;
        public static readonly string[] PackageDescription;

        public static readonly string[] PackageName;
        public static readonly string[] PackageVersion;

        public static string Local(string elementName)
        {
            return string.Format("*[local-name()='{0}']", elementName);
        }
    }
}