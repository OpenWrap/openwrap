using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Tests.resolving_dependency_trees
{
    [TestFixture("frodo", "sam")]
    [TestFixture("sam", "frodo")]
    class resolving_content_branches : contexts.dependency_resolver_context
    {
        public resolving_content_branches(string firstDependency, string secondDependency)
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
        public void dependent_package_exist_in_two_branches()
        {
            
        }
    }
}
