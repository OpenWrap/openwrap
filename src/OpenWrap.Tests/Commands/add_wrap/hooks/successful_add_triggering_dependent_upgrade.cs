using NUnit.Framework;
using OpenWrap;
using Tests.Commands.contexts;

namespace Tests.Commands.add_wrap.hooks
{
    class successful_add_triggering_dependent_upgrade : contexts.add_wrap_with_hooks
    {
        public successful_add_triggering_dependent_upgrade()
        {
            given_project_repository();
            given_project_package("one-ring", "1.0.0");
            given_dependency("depends: one-ring");

            given_remote_package("sauron", "1.0.0", "depends: one-ring = 1.1.0");
            given_remote_package("one-ring", "1.1.0");

            when_executing_command("sauron -project");
        }

        [Test]
        public void add_hook_is_called_for_new_dependency()
        {
            add_hook_should_be_called("project", "sauron", string.Empty, "1.0.0".ToSemVer());
        }

        [Test]
        public void update_hook_is_called_for_related_dependency()
        {
            update_hook_should_be_called("project", "one-ring", string.Empty, "1.0.0", "1.1.0");
        }
    }
}