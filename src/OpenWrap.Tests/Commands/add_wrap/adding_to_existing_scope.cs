using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using OpenWrap.PackageModel;
using OpenWrap.Testing;

namespace OpenWrap.Commands.add_wrap
{
    class adding_to_existing_scope : contexts.add_wrap_command
    {
        public adding_to_existing_scope()
        {
            given_system_package("sauron", "1.0.0");
            given_project_package("one-ring", "1.0.0");
            
            given_dependency("tests", "depends: one-ring");

            when_executing_command("sauron", "-scope", "tests");
        }

        [Test]
        public void default_descriptor_is_not_updated()
        {
            WrittenDescriptor().Dependencies.ShouldBeEmpty();
        }

        [Test]
        public void scoped_descriptor_is_updated()
        {
            WrittenDescriptor("tests").Dependencies.ShouldHaveCountOf(2);

        }
    }
}
