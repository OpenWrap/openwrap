using System;
using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.preloading.locating.project
{
    class missing_wrap_directory : contexts.preloader
    {
        public missing_wrap_directory()
        {
            given_project_directory();
            given_project_descriptor("project.wrapdesc");

            when_locating_package("bootstrap");
        }
        [Test]
        public void wraps_dir_is_created()
        {
            project_directory.GetDirectory("wraps").Exists.ShouldBeTrue();
        }
        [Test]
        public void wraps_cache_dir_is_created()
        {
            project_directory.GetDirectory("wraps").GetDirectory("_cache").Exists.ShouldBeTrue();
        }
    }
}