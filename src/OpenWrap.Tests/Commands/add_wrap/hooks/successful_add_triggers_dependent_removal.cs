using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenWrap;

namespace Tests.Commands.add_wrap.hooks
{
    class successful_add_triggers_dependent_removal : contexts.add_wrap_with_hooks
    {
        public successful_add_triggers_dependent_removal()
        {
            given_project_repository();
            given_project_package("one-ring", "1.0.0", "depends: fire = 1.0");
            given_project_package("fire", "1.0");
            given_dependency("depends: one-ring");

            given_remote_package("sauron", "1.0.0".ToVersion(), "depends: one-ring = 1.1.0");
            given_remote_package("one-ring", "1.1.0".ToVersion());

            when_executing_command("sauron -project");
        }

        [Test]
        public void new_dependency_is_added()
        {
            add_hook_should_be_called("project", "sauron", string.Empty, "1.0.0".ToSemVer());
        }

        [Test]
        public void dependent_dependency_is_updated()
        {
            update_hook_should_be_called("project", "one-ring", string.Empty, "1.0.0", "1.1.0");
        }

        [Test]
        public void unused_dependency_is_removed()
        {
            remove_hook_should_be_called("project", "fire", string.Empty, "1.0".ToSemVer());
        }
    }
}
