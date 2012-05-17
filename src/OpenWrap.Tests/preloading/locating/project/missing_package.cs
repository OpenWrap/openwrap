using NUnit.Framework;
using OpenWrap.IO.Packaging;
using OpenWrap.PackageManagement;
using OpenWrap.PackageModel;
using OpenWrap.Testing;

namespace Tests.preloading.locating.project
{
    class missing_package : contexts.preloader
    {
        public missing_package()
        {
            given_project_directory();
            given_project_descriptor("project.wrapdesc");

            when_locating_package("bootstrap");
        }

        [Test]
        public void exception_is_raised()
        {
            var missingPackages = exception.ShouldBeOfType<PackageMissingException>();
            missingPackages.Paths.ShouldHaveOne(
                project_directory.GetDirectory("wraps").GetDirectory("_cache").Path.FullPath);
        }
    }
}