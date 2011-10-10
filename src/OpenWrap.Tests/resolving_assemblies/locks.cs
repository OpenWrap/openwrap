using NUnit.Framework;
using OpenWrap.Testing;


namespace Tests.resolving_assemblies
{
    public class locks : contexts.assembly_resolving
    {
        public locks()
        {
            given_dependency("depends: east-bight");
            given_project_package("east-bight", "1.0.0", Assemblies(Assembly("eastbight", "bin-net35")));
            given_project_package("east-bight", "1.0.1", Assemblies(Assembly("eastbight", "bin-net35"), Assembly("mirkwood", "bin-net35")));
            given_environment("anyCPU", "net35");
            given_locked_package("east-bight", "1.0.0");
            when_resolving_assemblies();
        }
        [Test]
        public void locked_version_is_respected()
        {
            AssemblyReferences.ShouldHaveCountOf(1);
        }

    }
}