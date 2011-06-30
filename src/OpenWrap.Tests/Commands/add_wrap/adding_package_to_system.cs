using NUnit.Framework;
using OpenWrap;

namespace Tests.Commands.add_wrap
{
    class adding_package_to_system : contexts.add_wrap
    {
        public adding_package_to_system()
        {
            given_remote_package("sauron", "1.0.0".ToVersion());
            given_project_repository(null);
            given_default_descriptor(null);
            when_executing_command("sauron -system");
        }

        [Test]
        public void the_command_is_successful()
        {
            Results.ShouldHaveNoError();
        }
    }
}