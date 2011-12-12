using System;
using System.Linq;
using NUnit.Framework;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.Dependencies.trees
{
    public class dependency_stacks : dependency_resolver_context
    {
        public dependency_stacks()
        {
            given_project_package("openwrap-1.0.0.1");
            given_project_package("openfilesystem-1.0.0.0", "depends: openwrap");
            
            given_dependency("depends: openwrap");
            given_dependency("depends: openfilesystem");

            when_resolving_packages();
        }
        [Test]
        public void resolve_succeeds()
        {
            Resolve.IsSuccess.ShouldBeTrue();
        }
        [Test]
        public void direct_path_is_present()
        {
            Resolve.SuccessfulPackages.First(x => x.Identifier.Name == "openwrap")
                    .DependencyStacks.ShouldHaveAtLeastOne(x => x.ToString() == "openwrap -> openwrap-1.0.0+1");
        }

        [Test]
        public void indirect_path_is_present()
        {
            Resolve.SuccessfulPackages.First(x => x.Identifier.Name == "openwrap")
                    .DependencyStacks.ShouldHaveAtLeastOne(x => x.ToString() == "openfilesystem -> openfilesystem-1.0.0+0 -> openwrap -> openwrap-1.0.0+1");
        }
    }
}