using System.Linq;
using NUnit.Framework;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Commands.add_wrap
{
    public class adding_package_updates_descriptor : contexts.add_wrap
    {
        public adding_package_updates_descriptor()
        {
            given_project_repository(new InMemoryRepository("ProjectRepository"));
            given_system_package("sauron", "1.0.0.0");

            when_executing_command("sauron -content");   
        }

        [Test]
        public void descriptor_is_updated()
        {
            Environment.Descriptor.Dependencies.ShouldHaveCountOf(1)
                    .First().Name.ShouldBe("sauron");

        }
        [Test]
        public void descriptor_file_is_saved()
        {
            PostExecutionDescriptor.Dependencies.ShouldHaveCountOf(1)
                    .First().Name.ShouldBe("sauron");

        }
    }
}