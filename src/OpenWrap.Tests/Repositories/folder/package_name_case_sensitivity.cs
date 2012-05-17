using System;
using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Repositories.folder
{
    public class package_name_case_sensitivity : context.folder_based_repository
    {
        public package_name_case_sensitivity()
        {
            given_folder_repository_with_module("test-package");
            when_reading_test_module_descriptor("Test-Package");
        }
        [Test]
        public void package_is_found_case_insensitively()
        {
            Descriptor.ShouldNotBeNull();
        }
    }
}
