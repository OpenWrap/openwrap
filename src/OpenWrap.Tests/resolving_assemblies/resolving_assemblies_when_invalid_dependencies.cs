using System.Linq;
using NUnit.Framework;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.resolving_assemblies
{
    public class resolving_assemblies_when_invalid_dependencies : assembly_resolving
    {
        public resolving_assemblies_when_invalid_dependencies()
        {
            given_dependency("depends: mirkwood");
            given_dependency("depends: eastbight");
            given_project_package("mirkwood", "1.0.0.0", Assemblies(Assembly("mirkwood", "bin-net35")));
            given_environment("anyCPU", "net35");

            when_resolving_assemblies();
        }

        [Test]
        public void assemblies_from_valid_packages_are_loaded()
        {
            AssemblyReferences.ShouldHaveCountOf(1).First().AssemblyName.Name.ShouldBe("mirkwood");
        }
    }
}