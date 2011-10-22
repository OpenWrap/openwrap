using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands.Wrap;
using OpenWrap.Testing;

namespace Tests.Commands.list_wrap.project.detailed
{
    public class string_formatting : command<ListWrapCommand>
    {
        public string_formatting()
        {
            given_project_package("mount-doom", "2.0.0", "title: The mount of doom");
            given_project_package("sauron", "1.0.0", "depends: one-ring");
            given_project_package("one-ring", "1.0.0", "depends: mount-doom >= 2.0");
            given_project_package("frodo", "2.0.0");
            given_dependency("depends: sauron = 1.0.0");
            given_dependency("depends: frodo");
            given_dependency("depends: mount-doom");
            when_executing_command("-detailed");
        }
        [Test]
        public void output_is_correct()
        {

            Results.OfType<DescriptorPackages>().First().ToString()
                .ShouldBe(
                    @"─default scope
 ├─depends: sauron = 1.0.0
 │ └─sauron 1.0.0
 │   └─depends: one-ring
 │     └─one-ring 1.0.0
 │       └─depends: mount-doom >= 2.0
 │         └─mount-doom 2.0.0  (The mount of doom)
 ├─depends: frodo
 │ └─frodo 2.0.0
 └─depends: mount-doom
   └─mount-doom 2.0.0 (The mount of doom)"
                );
        }
    }
}