using System.Linq;
using NUnit.Framework;
using OpenWrap.Testing;

namespace OpenWrap.Commands.add_wrap
{
    class adding_to_new_scope : contexts.add_wrap_command
    {
        public adding_to_new_scope()
        {
            given_system_package("sauron", "1.0.0");
            given_project_package("one-ring", "1.0.0");
            
            given_dependency("depends: one-ring");

            when_executing_command("sauron", "-scope", "tests");
        }
        [Test]
        public void should_succeed()
        {
            Results.ShouldHaveNoError();
        }

        [Test]
        public void default_descriptor_is_not_updated()
        {
            WrittenDescriptor().Dependencies.ShouldHaveCountOf(1)
                    .First().Name.ShouldBe("one-ring");
        }

        [Test]
        public void scoped_descriptor_is_updated()
        {
            WrittenDescriptor("tests").Dependencies.ShouldHaveCountOf(1)
                    .First().Name.ShouldBe("sauron");
        }
    }
}