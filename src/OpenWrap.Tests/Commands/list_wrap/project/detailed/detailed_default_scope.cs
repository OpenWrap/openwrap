using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands.Wrap;
using OpenWrap.Testing;

namespace Tests.Commands.list_wrap.project.detailed
{
    public class detailed_default_scope : command<ListWrapCommand>
    {
        public detailed_default_scope()
        {
            given_project_package("sauron", "1.0.0", "depends: one-ring");
            given_project_package("one-ring", "1.0.0");
            given_dependency("depends: sauron = 1.0.0");
            when_executing_command("-detailed");
        }
        [Test]
        public void detailed_is_set()
        {
            Results.ShouldHaveOne<DescriptorPackages>().Detailed.ShouldBeTrue();
        }
        [Test]
        public void spec_is_displayed()
        {
            Results.OfType<DescriptorPackages>().First().ToString()
                .ShouldContain("depends: sauron = 1.0.0");
        }
        [Test]
        public void dependent_is_present()
        {
            Results.OfType<DescriptorPackages>().First()
                .Packages.First().Children.First().PackageInfo.Name.ShouldBe("one-ring");
        }
        [Test]
        public void dependent_is_displayed()
        {
            Results.OfType<DescriptorPackages>().First().ToString()
                .ShouldContain("one-ring 1.0.0");
        }
    }
}