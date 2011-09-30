using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Build.builders.msbuild
{
    public class project_use_recursive_wildcard : contexts.msbuild_builder
    {
        public project_use_recursive_wildcard()
        {
            given_empty_directory();
            given_file(_ => _.GetDirectory("source"), "test1.csproj");
            given_file(_ => _.GetDirectory("source"), "test2.csproj");
            when_building("**" + Path.DirectorySeparatorChar + "*.csproj");
        }
        [Test]
        public void project_files_are_found()
        {
            Builder.ProjectFiles.ShouldContain(rootPath.GetDirectory("source").GetFile("test1.csproj"));
            Builder.ProjectFiles.ShouldContain(rootPath.GetDirectory("source").GetFile("test2.csproj"));
        }
    }
}