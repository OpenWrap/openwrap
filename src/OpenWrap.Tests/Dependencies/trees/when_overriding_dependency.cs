using System.Linq;
using NUnit.Framework;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.Dependencies.trees
{
    public class when_overriding_dependency : dependency_resolver_context
    {
        public when_overriding_dependency()
        {
            given_remote1_package("one-ring-1.0.0");
            given_remote1_package("sauron-1.0.0", "depends: ring-of-power");

            given_project_package("minas-tirith-1.0.0");


            given_dependency("depends: sauron");
            given_dependency("depends: fangorn");


            given_dependency_override("ring-of-power", "one-ring");
            given_dependency_override("fangorn", "minas-tirith");

            when_resolving_packages();
        }

        [Test]
        public void dependencies_in_dependency_chain_are_overridden()
        {
            Resolve.SuccessfulPackages.Where(x => x.Identifier.Name == "one-ring").FirstOrDefault()
                .ShouldNotBeNull()
                .Packages.First().Name.ShouldBe("one-ring");
        }

        [Test]
        public void locally_declared_dependency_is_overrridden()
        {
            Resolve.SuccessfulPackages.Where(x => x.Identifier.Name == "minas-tirith").FirstOrDefault()
                .ShouldNotBeNull()
                .Packages.First().Name.ShouldBe("minas-tirith");
        }

        [Test]
        public void originally_declared_dependency_in_dependency_chain_is_not_resolved()
        {
            Resolve.SuccessfulPackages.Where(x => x.Identifier.Name == "ring-of-power")
                .ShouldHaveCountOf(0);
        }

        [Test]
        public void originally_locally_declared_dependency_is_not_resolved()
        {
            Resolve.SuccessfulPackages.Where(x => x.Identifier.Name == "fangorn")
                .ShouldHaveCountOf(0);
        }

        [Test]
        public void resolution_is_successfull()
        {
            Resolve.IsSuccess.ShouldBeTrue();
        }
    }
}