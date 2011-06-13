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
        public void scope_for_absolute_path_is_found()
        {
            scope_should_be(@"c:\src\tests\folder", "tests", @"{source: src}\{scope: tests}");
        }

        [Test]
        public void scope_for_relative_path_throws_error()
        {
            Executing(()=>PathFinder.GetCurrentScope(new[] { @"{source: src}\{scope: tests}" }, new Path(@"src/project.csproj")))
                    .ShouldThrow<ArgumentException>();
        }

        [Test]
        public void scope_for_nested_duplicated_paths_found()
        {
            scope_should_be(@"c:\src\tests\src\MyProject.Tests\MyProject.Tests.csproj", "tests", @"{source: src}\*{scope: Tests = tests}*");
        }

        [TestCase(@"c:\src\tests\src\MyProject.Tests\MyProject.Tests.csproj")]
        [TestCase(@"c:\src\tests\src\MyProject.Tests.Unit\MyProject.Tests.Unit.csproj")]
        public void scope_without_source_is_found(string path)
        {
            scope_should_be(path, "tests", @"src\*{scope: Tests = tests}*");
        }
    }
    namespace contexts
    {
        public abstract class scope : context
        {
            protected void scope_should_be(string filePath, string expectedScope, params string[] templates)
            {
                PathFinder.GetCurrentScope(templates, new Path(filePath)).ShouldBe(expectedScope);
            }
        }
    }
}
