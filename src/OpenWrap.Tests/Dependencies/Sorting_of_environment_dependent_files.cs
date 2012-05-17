using NUnit.Framework;
using OpenWrap.PackageManagement.Exporters.Assemblies;
using Tests.Dependencies.parser;

namespace Tests.Dependencies
{
    public class Sorting_of_environment_dependent_files
    {
        [Test]
        public void elements_are_sorted_by_target_first()
        {
            File("AnyCPU", "net35").ShouldBeBefore(File("x86", "net30"));
        }

        [Test]
        public void elements_with_same_target_are_sorted_for_platform_first()
        {
            File("AnyCPU", "net30").ShouldBeAfter(File("x86", "net30"));
        }

        public void elements_with_specific_architectures_win()
        {
            File("x86", "net35").ShouldBeBefore(File("x86", "net20"));
        }

        AbstractAssemblyExporter.EnvironmentDependentFile File(string platform, string target)
        {
            return new AbstractAssemblyExporter.EnvironmentDependentFile
            {
                Platform = platform,
                Profile = target
            };
        }
    }
}