using NUnit.Framework;
using OpenWrap.Testing;
using Tests.Repositories.context;

namespace Tests.Repositories
{
    public class finding_a_package : current_directory_repository
    {
        public finding_a_package()
        {
            given_file_system(@"c:\mordor\");
            given_current_folder_repository();
            given_packages(Package("isenmouthe", "1.0.0"));
            given_current_folder_repository();
            when_finding_packages("depends: isenmouthe");
        }

        [Test]
        public void the_package_is_found()
        {
            FoundPackage.ShouldNotBeNull()
                .Name.ShouldBe("isenmouthe");
        }
    }
}