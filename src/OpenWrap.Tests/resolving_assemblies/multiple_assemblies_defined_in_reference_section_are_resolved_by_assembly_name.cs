using System.Linq;
using NUnit.Framework;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.resolving_assemblies
{
    public class multiple_assemblies_defined_in_reference_section_are_resolved_by_assembly_name : assembly_resolving
    {
        public multiple_assemblies_defined_in_reference_section_are_resolved_by_assembly_name()
        {
            given_dependency("depends: east-bight");
            given_project_package("east-bight", "1.0.0.0",
                                  Assemblies(Assembly("eastbight", "bin-net35"), Assembly("eastbight.runner", "bin-net35")),
                                  "referenced-assemblies: eastbight, eastbight.runner");
            given_environment("anyCPU", "net35");

            when_resolving_assemblies();
        }

        [Test]
        public void included_assembly_is_referenced()
        {
            AssemblyReferences.ShouldHaveCountOf(2)
                    .Check(x=>x.Any(asm=>asm.AssemblyName.Name == "eastbight").ShouldBeTrue())
                    .Check(x=>x.Any(asm=>asm.AssemblyName.Name == "eastbight.runner").ShouldBeTrue());
        }
    }
}