using NUnit.Framework;
using OpenWrap.Build;
using OpenWrap.Testing;

namespace Tests.Build.builders.msbuild
{
    public class explicit_projects_without_source_folder : contexts.msbuild_builder
    {
        public explicit_projects_without_source_folder()
        {
            given_empty_directory();
            given_file("test1.csproj");

            when_building("test1.csproj");
        }
        [Test]
        public void source_folder_is_required()
        {
            SrcRelatedErrorResults.ShouldBeEmpty();
        }
    }
}