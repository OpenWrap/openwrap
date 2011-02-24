using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenWrap.IO;
using OpenWrap.Testing;

namespace Tests.IO
{
    public class determining_scope : contexts.scope
    {
        [Test]
        public void scope_with_paths_is_found()
        {
            scope_should_be(@"c:\src\tests\folder", "tests", @"{source: src}\{scope: tests}");
        }
    }
    namespace contexts
    {
        public abstract class scope : context
        {
            protected void scope_should_be(string file, string expectedScope, params string[] templates)
            {
                PathFinder.GetCurrentScope(templates, new Path(file)).ShouldBe(expectedScope);
            }
        }
    }
}
