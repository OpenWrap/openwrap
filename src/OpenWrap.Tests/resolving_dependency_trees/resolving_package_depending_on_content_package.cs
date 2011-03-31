using System.Linq;
using NUnit.Framework;
using OpenWrap.PackageManagement.DependencyResolvers;
using OpenWrap.PackageModel;
using OpenWrap.Testing;

namespace Tests.resolving_dependency_trees
{
    class resolving_package_depending_on_content_package : contexts.dependency_resolver_context
    {
        public resolving_package_depending_on_content_package()
        {
            given_project_package("barad-dur-1.0", "depends: one-ring");
            given_project_package("one-ring-1.0");
            given_project_package("sauron-1.0");
            given_dependency("depends: barad-dur content");
            given_dependency("depends: sauron");

            when_resolving_packages();
        }

        [Test]
        public void dependencies_are_resolved_successfully()
        {
            Resolve.IsSuccess.ShouldBeTrue();
        }

        [Test]
        public void packages_marked_as_content_are_in_content_branch()
        {
            Resolve.InContentBranch().ShouldHaveCountOf(2)
                    .ToLookup(x => x.Name)
                    .Check(x => x["barad-dur"].ShouldHaveCountOf(1))
                    .Check(x => x["one-ring"].ShouldHaveCountOf(1));

        }

        [Test]
        public void packages_not_marked_as_content_are_not_in_content_branch()
        {
            Resolve.NotInContentBranch().ShouldHaveCountOf(1)
                .First().Name.ShouldBe("sauron");            
        }
    }
}