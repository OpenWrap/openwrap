using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.Commands;
using OpenWrap.Commands.Wrap;
using NUnit.Framework;
using OpenWrap.IO;
using System.IO;

namespace OpenWrap.Tests.Commands
{
    public class when_removing_wrap : context.command_context<RemoveWrapCommand>
    {
        public when_removing_wrap()
        {
            using (var f = Environment.CurrentDirectory.GetFile("descriptor.wrapdesc").OpenWrite())
            using (var w = new StreamWriter(f))
            {
                w.Write("depends bar\r\ndepends foo");
            }

            given_dependency("depends foo");
            when_executing_command("foo");
        }

        [Test]
        public void dependency_is_removed()
        {
            Assert.AreEqual(0, Environment.Descriptor.Dependencies.Count);

            using (var f = Environment.CurrentDirectory.GetFile("descriptor.wrapdesc").OpenRead())
            using (var r = new StreamReader(f))
            {
                Assert.AreEqual("depends bar", r.ReadLine());
                Assert.AreEqual(null, r.ReadLine());
            }
        }
    }

}
