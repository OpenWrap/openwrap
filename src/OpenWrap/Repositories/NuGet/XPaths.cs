namespace OpenWrap.Repositories.NuGet
{
    public static class XPaths
    {
        public static string Local(string elementName)
        {
            return string.Format("*[local-name()='{0}']", elementName);
        }

        public static string Metadata = "//" + Local("package") + "/" + Local("metadata") + "/";
        public static string Dependencies = Metadata + Local("dependencies") + "/" + Local("dependency");

        public static string[] PackageName = new[] { Metadata + Local("id") };
        public static string[] PackageVersion = new[] { Metadata + Local("version") };
        public static string[] PackageDescription = new[] { Metadata + Local("description") };

        public static string[] PackageDependencies = new[] { Dependencies };
    }
}