using NUnit.Framework;
using OpenWrap.Commands.contexts;
using OpenWrap.Testing;

namespace OpenWrap.Commands.remove_wrap
{
    public class removing_from_existing_scope : remove_wrap_command
    {
        public removing_from_existing_scope()
        {
            given_project_package("one-ring", "1.0.0");
                
            given_dependency("tests", "one-ring");
            when_executing_command("one-ring -scope tests");
        }

        [Test]
        public void default_scope_is_preserved()
        {
            WrittenDescriptor().Dependencies.ShouldBeEmpty();
        }

        [Test]
        public void specific_scope_is_updated()
        {
            WrittenDescriptor("tests").Dependencies.ShouldBeEmpty();
        }

        [Test]
        public void default_scope_is_not_updated()
        {
            WrittenDescriptor().Dependencies.ShouldBeEmpty();
        }
    }
}