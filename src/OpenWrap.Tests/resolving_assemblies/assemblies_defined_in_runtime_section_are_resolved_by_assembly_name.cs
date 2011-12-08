using System.Linq;
using NUnit.Framework;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.resolving_assemblies
{
    [TestFixture("eastbight", "eastbight.runner")]
    [TestFixture("eastbight.dll", "eastbight.runner.dll")]
    public class assemblies_defined_in_runtime_section_are_resolved_by_assembly_name : assembly_resolving
    {
        public assemblies_defined_in_runtime_section_are_resolved_by_assembly_name(string assemblyName, string runtimeAssemblyName)
        {
            given_dependency("depends: east-bight");
            given_project_package("east-bight", "1.0.0.0", Assemblies(Assembly("eastbight", "bin-net35"), Assembly("eastbight.runner", "bin-net35")),
                                  "referenced-assemblies: " + assemblyName, "runtime-assemblies: " + runtimeAssemblyName);
            given_environment("anyCPU", "net35");
            
            when_resolving_assemblies();
        }

        [Test]
        public void included_assembly_is_referenced()
        {
            AssemblyReferences.ShouldHaveCountOf(2).ShouldHaveOne(a => a.AssemblyName.Name == "eastbight" && !a.IsRuntimeAssembly && a.IsAssemblyReference);
        }

        [Test]
        public void included_runtime_assembly_is_referenced()
        {
            AssemblyReferences.ShouldHaveCountOf(2).ShouldHaveOne(a => a.AssemblyName.Name == "eastbight.runner" && a.IsRuntimeAssembly && !a.IsAssemblyReference);
        }
    }
}