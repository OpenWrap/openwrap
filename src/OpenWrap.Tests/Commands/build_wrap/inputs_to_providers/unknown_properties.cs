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
                                 Version = "1.0.0.0".ToVersion(),
                                 Build = { "custom;unknown=value;unknown=value2;unknown2=value;typename=" + typeof(PackageBuilderWithConfig).AssemblyQualifiedName }
                             });
            when_executing_command();
        }

        [Test]
        public void unknown_values_are_set()
        {
            PackageBuilderWithConfig.PropertiesValue
                .Check(val=>val.ElementAt(0).Check(_=>_.Key.ShouldBe("unknown")).ShouldBe("value", "value2"))
                .Check(val=>val.ElementAt(1).Check<IGrouping<string, string>>(_=>_.Key.ShouldBe("unknown2")).ShouldBe("value"));
        }
    }
}