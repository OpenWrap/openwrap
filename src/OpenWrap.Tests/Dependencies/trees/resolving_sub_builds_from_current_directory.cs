using System.Linq;
using NUnit.Framework;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.Dependencies.trees
{
    public class resolving_sub_builds_from_current_directory : dependency_resolver_context
    {
        public resolving_sub_builds_from_current_directory()
        {
            given_project_package("rings-of-power-1.0.0");
            given_current_directory_package("rings-of-power-1.0.0.1");
            given_dependency("depends: rings-of-power");

            when_resolving_packages();
        }

        [Test]
        public void package_found_from_current_directory()
        {
            Resolve.SuccessfulPackages.Where(x => x.Identifier.Name == "rings-of-power")
                .ShouldHaveCountOf(1)
                .First().Packages.First().Source.ShouldBe(CurrentDirectoryRepository);
        }

    }
}