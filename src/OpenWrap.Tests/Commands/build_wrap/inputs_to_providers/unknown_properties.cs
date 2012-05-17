using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Commands.build_wrap.inputs_to_providers
{
    public class unknown_properties : contexts.build_wrap
    {
        
        public unknown_properties()
        {
            given_current_directory_repository(new CurrentDirectoryRepository());
            given_descriptor(FileSystem.GetCurrentDirectory(),
                             new PackageDescriptor
                             {
                                 Name = "test",
                                 SemanticVersion = "1.0.0.0".ToSemVer(),
                                 Build = { "custom;unknown=value;unknown=value2;unknown2=value;typename=" + typeof(PackageBuilderWithConfig).AssemblyQualifiedName }
                             });
            when_executing_command();
        }

        [Test]
        public void unknown_values_are_set()
        {
            PackageBuilderWithConfig.PropertiesValue
                .Check(val=>val["unknown"].ShouldBe("value", "value2"))
                .Check(val=>val["unknown2"].ShouldBe("value"));
        }
    }
}
