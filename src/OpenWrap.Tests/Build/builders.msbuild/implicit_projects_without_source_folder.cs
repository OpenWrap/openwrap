using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Build.builders.msbuild
{
    public class implicit_projects_without_source_folder : contexts.msbuild_builder
    {
        public implicit_projects_without_source_folder()
        {
            given_empty_directory();
            given_file("test1.csproj");

            when_building_package();
        }

        [Test]
        public void soource_folder_is_not_required()
        {
            SrcRelatedErrorResults.ShouldNotBeEmpty();
        }
    }
}