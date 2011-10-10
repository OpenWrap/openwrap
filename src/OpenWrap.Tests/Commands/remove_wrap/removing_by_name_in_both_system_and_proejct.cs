using System.Linq;
using NUnit.Framework;
using OpenWrap.PackageModel;
using OpenWrap.Testing;

namespace Tests.Commands.remove_wrap
{
    public class removing_by_name_in_both_system_and_proejct : global::Tests.Commands.contexts.remove_wrap
    {
        public removing_by_name_in_both_system_and_proejct()
        {
            given_dependency("depends: gandalf");
            given_system_package("gandalf", "1.0.0.0");
            given_project_package("saruman", "99");
            given_project_package("gandalf", "1.0.0.0");
            when_executing_command("gandalf -project -system");
        }
        [Test]
        public void package_removed_from_both_repositories()
        {
            SpecExtensions.ShouldBeEmpty<PackageDependency>(PostCommandDescriptor.Dependencies
                                                              .Where(x => x.Name == "galdalf"));
            Environment.SystemRepository.PackagesByName["gandalf"]
                    .ShouldBeEmpty();
        }
    }
}