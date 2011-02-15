using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenWrap.Testing;

namespace OpenWrap.Commands.add_wrap.hooks
{
    class successful_add_to_project : contexts.add_wrap_with_hooks
    {
        public successful_add_to_project()
        {
            given_project_repository();
            given_remote_package("sauron", "1.0.0");

            when_executing_command("sauron", "-project");
        }

        [Test]
        public void add_hook_is_called()
        {
            add_hook_should_be_called("project", "sauron", string.Empty, "1.0.0".ToVersion());
        }

        [Test]
        public void add_hook_called_once()
        {
            AddCalls.ShouldHaveCountOf(1);
        }

        [Test]
        public void no_update_hook_called()
        {
            UpdateCalls.ShouldBeEmpty();
        }

        [Test]
        public void no_remove_hook_called()
        {
            RemoveCalls.ShouldBeEmpty();
        }
    }
}
