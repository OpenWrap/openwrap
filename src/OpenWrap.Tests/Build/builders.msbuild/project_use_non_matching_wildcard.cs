using System.Linq;
using NUnit.Framework;
using OpenWrap.Build.PackageBuilders;
using OpenWrap.Testing;

namespace Tests.Build.builders.msbuild
{
    public class project_use_non_matching_wildcard : contexts.msbuild_builder
    {
        public project_use_non_matching_wildcard()
        {
            given_empty_directory();
            given_file(_ => _.GetDirectory("source"), "test1.csproj");
            when_building("*.csproj");
        }
        [Test]
        public void project_file_is_not_found()
        {
            Builder.ProjectFiles.ShouldBeEmpty();
        }

        [Test]
        public void error_is_reported()
        {
            Results.OfType<UnknownProjectFileResult>().ShouldNotBeEmpty()
                .First().Spec.ShouldBe("*.csproj");
        }
    }
}