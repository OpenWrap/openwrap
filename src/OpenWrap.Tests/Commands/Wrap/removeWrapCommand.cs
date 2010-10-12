using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenWrap.Commands.Wrap;
using OpenWrap.Dependencies;
using OpenWrap.Testing;

namespace OpenWrap.Tests.Commands
{
    public class removing_wrap : context.remove_wrap
    {
        public removing_wrap()
        {
            given_dependency("depends: bar");
            given_dependency("depends: foo");
            given_project_package("foo", "1.0.0.0".ToVersion());
            given_project_package("bar", "1.0.0.0".ToVersion());

            when_executing_command("foo");

        }


        [Test]
        public void dependency_is_removed_from_descriptor()
        {
            PostCommandDescriptor.Dependencies.ShouldHaveCountOf(1);
        }

        [Test]
        public void package_removed_from_descriptor()
        {
            PostCommandDescriptor.Dependencies.Where(x=>x.Name == "foo").ShouldBeEmpty();
        }

        [Test]
        public void unaffected_packages_remain()
        {
            PostCommandDescriptor.Dependencies.Where(x => x.Name != "foo").ShouldHaveCountOf(1);
        }
    }

    namespace context
    {
        public abstract class remove_wrap : command_context<RemoveWrapCommand>
        {

            protected PackageDescriptor PostCommandDescriptor;
            protected override void when_executing_command(params string[] parameters)
            {
                base.when_executing_command(parameters);
                PostCommandDescriptor = new PackageDescriptorReaderWriter().Read(Environment.DescriptorFile);
            }
        }
    }
}
