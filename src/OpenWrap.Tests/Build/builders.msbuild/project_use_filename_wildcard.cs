using System.Linq;
using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Build.builders.msbuild
{
    public class project_use_filename_wildcard : contexts.msbuild_builder
    {
        public project_use_filename_wildcard()
        {
            given_empty_directory();
            given_file("test1.csproj");
            when_building("*.csproj");
        }
        [Test]
        public void project_file_is_found()
        {
            Builder.ProjectFiles.Single().ShouldBe(rootPath.GetFile("test1.csproj"));
        }
    }
}