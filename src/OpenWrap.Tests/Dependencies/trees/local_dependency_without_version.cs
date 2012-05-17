using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.Dependencies.trees
{
    public class local_dependency_without_version : dependency_resolver_context
    {
        public local_dependency_without_version()
        {
            given_remote1_package("nhibernate-2.0.0");
            given_remote1_package("nhibernate-1.0.0");
            given_remote1_package("fluent-nhibernate-1.0.0", "depends: nhibernate = 1.0.0");


            given_dependency("depends: fluent-nhibernate");
            given_dependency("depends: nhibernate");

            when_resolving_packages();
        }

        [Test]
        public void local_choice_doesnt_override()
        {
            Resolve.SuccessfulPackages.Where(x => x.Identifier.Name == "nhibernate")
                .ShouldHaveCountOf(1)
                .First().Identifier.Version.ShouldBe("1.0.0");
        }
    }
}