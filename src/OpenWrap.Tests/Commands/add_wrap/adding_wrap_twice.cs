using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands.contexts;
using OpenWrap.Commands.Wrap;
using OpenWrap.Testing;

namespace Tests.Commands.add_wrap
{
    class adding_wrap_twice : command_context<AddWrapCommand>
    {
        public adding_wrap_twice()
        {
            given_dependency("depends: sauron");
            given_project_package("sauron", "1.0.0.0");

            when_executing_command("sauron -content");
        }

        [Test]
        public void one_entry_exists()
        {
            Environment.Descriptor.Dependencies.Select(x => x.Name == "sauron")
                    .ShouldHaveCountOf(1);
        }

        [Test]
        public void entry_is_overwritten()
        {
            Environment.Descriptor.Dependencies.Single()
                    .ContentOnly.ShouldBeTrue();
        }
    }
}
