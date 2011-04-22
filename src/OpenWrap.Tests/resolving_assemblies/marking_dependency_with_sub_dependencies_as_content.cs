using NUnit.Framework;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.resolving_assemblies
{
    public class marking_dependency_with_sub_dependencies_as_content : assembly_resolving
    {
        public marking_dependency_with_sub_dependencies_as_content()
        {
            given_dependency("depends: mirkwood content");
            given_project_package("east-bight", "1.0.0.0", Assemblies(Assembly("esatbight", "bin-net35")));
            given_project_package("mirkwood", "1.0.0.0", Assemblies(Assembly("mirkwood", "bin-net35")), "depends: east-bight");
            given_environment("anyCPU", "net35");

            when_resolving_assemblies();
        }
        [Test]
        public void assemblies_are_ignored()
        {
            AssemblyReferences.ShouldBeEmpty();
        }
    }
}