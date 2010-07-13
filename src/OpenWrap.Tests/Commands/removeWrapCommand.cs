using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands.Wrap;
using OpenWrap.IO;

namespace OpenWrap.Tests.Commands
{
    public class when_removing_wrap_foo : context.command_context<RemoveWrapCommand>
    {
        public when_removing_wrap_foo()
        {
            using (var f = Environment.CurrentDirectory.GetFile("descriptor.wrapdesc").Open(FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var w = new StreamWriter(f))
            {
                w.Write("depends bar\r\ndepends foo");
            }

            given_dependency("depends bar");
            given_dependency("depends foo");
            when_executing_command("foo");

            using (var f = Environment.CurrentDirectory.GetFile("descriptor.wrapdesc").OpenRead())
            using (var r = new StreamReader(f))
            {
                descriptorFileLinesAfterRemove = r.ReadToEnd().Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        string[] descriptorFileLinesAfterRemove;

        [Test]
        public void dependency_is_removed_from_descriptor()
        {
            Assert.AreEqual(1, Environment.Descriptor.Dependencies.Count);
            Assert.AreEqual("bar", Environment.Descriptor.Dependencies.First().Name);
        }

        [Test]
        public void wrap_foo_is_removed()
        {
            Assert.AreEqual(1, descriptorFileLinesAfterRemove.Length);
        }

        [Test]
        public void wrap_bar_remains()
        {
            Assert.AreEqual("depends bar", descriptorFileLinesAfterRemove[0]);
        }
    }

}
