using System.Linq;
using NUnit.Framework;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace OpenWrap.Commands.remove_wrap
{
    public class removing_by_name_in_project : remove_wrap_command
    {
        public removing_by_name_in_project()
        {
            given_dependency("depends: bar");
            given_dependency("depends: foo");
            given_project_package("foo", "1.0.0.0");
            given_project_package("bar", "1.0.0.0");

            when_executing_command("foo");
        }


        [Test]
        public void dependency_is_removed_from_descriptor()
        {
            PostCommandDescriptor.Dependencies.ShouldHaveCountOf(1);
        }

        [Test]
        public void package_removed_from_descriptor()
        {
            PostCommandDescriptor.Dependencies.Where(x=>x.Name == "foo").ShouldBeEmpty();
        }

        [Test]
        public void unaffected_packages_remain()
        {
            PostCommandDescriptor.Dependencies.Where(x => x.Name != "foo").ShouldHaveCountOf(1);
        }
    }
}