using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.Dependencies.trees
{
    public class dependencies_in_conflict_wtith_local_definition : dependency_resolver_context
    {
        public dependencies_in_conflict_wtith_local_definition()
        {
            given_project_package("log4net-1.0.0");

            given_remote1_package("castle.windsor-2.5.1", "depends: castle.core = 2.5.1");
            given_remote1_package("castle.windsor-2.1.1", "depends: castle.core");
            given_remote1_package("castle.core-2.5.1");
            given_remote1_package("castle.dynamicproxy-2.1.0", "depends: castle.core = 1.1.0");
            given_remote1_package("castle.dynamicproxy-2.2.0", "depends: castle.core = 1.2.0");
            given_remote1_package("castle.core-1.1.0", "depends: log4net");
            given_remote1_package("castle.core-1.2.0", "depends: log4net", "depends: NLog <= 1.0");
            given_remote1_package("castle.core-2.5.1");
            given_remote1_package("NHibernate.Core", "depends: log4net = 1.2.1");


            given_dependency("depends: castle.windsor = 2.1");
            given_dependency("depends: castle.core = 1.1");

            when_resolving_packages();
        }

        [Test]
        public void local_choice_overrides()
        {
            Resolve.SuccessfulPackages.Where(x => x.Identifier.Name == "castle.core")
                .ShouldHaveCountOf(1)
                .First().Identifier.Version.ShouldBe("1.1.0".ToSemVer());
        }
    }
}