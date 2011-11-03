using NUnit.Framework;
using OpenWrap.Testing;
using Tests.Repositories.context;

namespace Tests.Repositories
{
    public class when_reading_packages_by_name : current_directory_repository
    {
        public when_reading_packages_by_name()
        {
            given_file_system(@"c:\mordor\");
            given_current_folder_repository();

            given_packages(Package("isenmouthe", "1.0.0"), Package("gorgoroth", "2.0.0"));
            given_current_folder_repository();
            when_getting_package_names();
        }

        [Test]
        public void the_packages_are_available_by_name()
        {
            PackagesByName.Contains("isenmouthe").ShouldBeTrue();
            PackagesByName.Contains("gorgoroth").ShouldBeTrue();
        }
    }
}