using System.Linq;
using NUnit.Framework;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.resolving_assemblies
{
    public class assemblies_are_found : assembly_resolving
    {
        public assemblies_are_found()
        {
            given_dependency("depends: mirkwood");
            given_project_package("east-bight", "1.0.0.0", Assemblies(Assembly("eastbight", "bin-net35")));
            given_project_package("mirkwood", "1.0.0.0", Assemblies(Assembly("mirkwood", "bin-net35")), "depends: east-bight");
            given_environment("anyCPU", "net35");
            when_resolving_assemblies();
        }
        [Test]
        public void assemblies_are_ignored()
        {
            AssemblyReferences.ShouldHaveCountOf(2)
                    .Check(x => x.FirstOrDefault(y => y.AssemblyName.Name == "eastbight").ShouldNotBeNull())
                    .Check(x => x.FirstOrDefault(y => y.AssemblyName.Name == "mirkwood").ShouldNotBeNull());
        }
    }
}