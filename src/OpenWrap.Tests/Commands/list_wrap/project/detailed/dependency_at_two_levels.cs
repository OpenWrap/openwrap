using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands.Wrap;
using OpenWrap.Testing;

namespace Tests.Commands.list_wrap.project.detailed
{
    public class recursive_dependencies : command<ListWrapCommand>
    {
        // should be a stack to be reliable
        public recursive_dependencies()
        {
            given_project_package("sauron", "1.0.0", "depends: one-ring");
            given_project_package("one-ring", "1.0.0", "depends: sauron");
            
            given_dependency("depends: one-ring");
            when_executing_command("-detailed");
        }
        [Test]
        public void dependency_graph_goes_to_least_nested_node()
        {
            Results.OfType<DescriptorPackages>().Single()
                .Packages.First(x => x.PackageInfo.Name == "one-ring")
                .Children.First(x => x.PackageInfo.Name == "sauron")
                .Children.First(x=>x.PackageInfo.Name == "one-ring")
                .Children.ShouldBe(DescriptorPackages.Truncated);
        }
    }
}