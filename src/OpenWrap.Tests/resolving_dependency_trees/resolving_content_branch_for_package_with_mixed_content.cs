using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenWrap.PackageManagement.DependencyResolvers;
using OpenWrap.Testing;

namespace Tests.resolving_dependency_trees
{
    [TestFixture("frodo", "sam")]
    [TestFixture("sam", "frodo")]
    class resolving_content_branch_for_package_with_mixed_content : contexts.dependency_resolver_context
    {
        public resolving_content_branch_for_package_with_mixed_content(string firstDependency, string secondDependency)
        {
            given_project_package("barad-dur-1.0");
            given_project_package("one-ring-1.0", "depends: barad-dur");
            given_project_package("frodo-1.0", "depends: one-ring content");
            given_project_package("sam-1.0", "depends: one-ring");
            given_dependency("depends: " + firstDependency);
            given_dependency("depends: " + secondDependency);

            when_resolving_packages();
        }

        [Test]
        public void dependencies_are_resolved_successfully()
        {
            Resolve.IsSuccess.ShouldBeTrue();
        }
        [Test]
        public void package_is_not_in_content_branch()
        {
            Resolve.NotInContentBranch().ShouldHaveCountOf(4);
            Resolve.InContentBranch().ShouldHaveCountOf(0);
        }

    }
}
