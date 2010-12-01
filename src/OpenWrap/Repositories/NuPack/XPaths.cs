namespace OpenWrap.Repositories.NuPack
{
    public static class XPaths
    {
        public const string Metadata = "package/metadata/";
        public const string MetadataNS = "nuspec:package/nuspec:metadata/";

        public const string Dependencies = "package/dependencies/dependency";
        public const string DependenciesNS = "nuspec:package/nuspec:dependencies/nuspec:dependency";

        public static string[] PackageName = new[] { Metadata + "id", MetadataNS + "nuspec:id" };
        public static string[] PackageVersion = new[] { Metadata + "version", MetadataNS + "nuspec:version" };
        public static string[] packageDescrition = new[] { Metadata + "description", MetadataNS + "nuspec:description" };

        public static string[] PackageDependencies = new[] { Dependencies, DependenciesNS };
    }
}