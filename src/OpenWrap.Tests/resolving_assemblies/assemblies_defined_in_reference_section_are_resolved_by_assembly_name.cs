using System.Linq;
using NUnit.Framework;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.resolving_assemblies
{
    [TestFixture("eastbight")]
    [TestFixture("eastbight.dll")]
    public class assemblies_defined_in_reference_section_are_resolved_by_assembly_name : assembly_resolving
    {
        public assemblies_defined_in_reference_section_are_resolved_by_assembly_name(string assemblyName)
        {

            given_dependency("depends: east-bight");
            given_project_package("east-bight", "1.0.0.0", Assemblies(Assembly("eastbight", "bin-net35"), Assembly("eastbight.runner", "bin-net35")),
                "referenced-assemblies: " + assemblyName);
            given_environment("anyCPU", "net35");

            when_resolving_assemblies();
        }

        [Test]
        public void excluded_assembly_is_not_resolved()
        {
            AssemblyReferences.Any(x => x.AssemblyName.Name == "eastbight.runer").ShouldBeFalse();
        }

        [Test]
        public void included_assembly_is_referenced()
        {
            AssemblyReferences.ShouldHaveCountOf(1).First().AssemblyName.Name.ShouldBe("eastbight");
        }
    }
}