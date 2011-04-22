using NUnit.Framework;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.resolving_assemblies
{
    public class recursive_dependencies_for_assemblies : assembly_resolving
    {
        public recursive_dependencies_for_assemblies()
        {
            given_dependency("depends: openwrap content");
            given_dependency("depends: openfilesystem");
            given_dependency("depends: sharpziplib");
            given_project_package("openwrap", "1.0.0.0", Assemblies(Assembly("openwrap", "bin-net35")), "depends: sharpziplib", "depends: openfilesystem");
            given_project_package("sharpziplib", "1.0.0.0", Assemblies(Assembly("sharpziplib", "bin-net35")));
            given_project_package("openfilesystem", "1.0.0.0", Assemblies(Assembly("openfilesystem", "bin-net35")), "depends: openwrap content", "depends: sharpziplib");
            given_environment("anyCPU", "net35");

            when_resolving_assemblies();
        }
        [Test]
        public void assemblies()
        {
            AssemblyReferences.ShouldHaveCountOf(2);
        }
    }
}