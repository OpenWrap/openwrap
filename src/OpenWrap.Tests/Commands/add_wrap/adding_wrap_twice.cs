using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands.Wrap;
using OpenWrap.PackageManagement;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.add_wrap
{
    class adding_wrap_twice : command<AddWrapCommand>
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
        public void error_is_displayed()
        {
            Results.ShouldHaveOne<PackageDependencyAlreadyExists>()
                .PackageName.ShouldBe("sauron");
        }
    }
}
