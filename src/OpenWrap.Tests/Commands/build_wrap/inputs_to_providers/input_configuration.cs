using NUnit.Framework;
using OpenWrap;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Commands.build_wrap.inputs_to_providers
{
    public class input_configuration : contexts.build_wrap
    {
        public input_configuration()
        {

            given_current_directory_repository(new CurrentDirectoryRepository());
            given_descriptor(FileSystem.GetCurrentDirectory(),
                             new PackageDescriptor
                             {
                                 Name = "test",
                                 Version = "1.0.0.0".ToVersion(),
                                 Build = { "custom;typename=" + typeof(PackageBuilderWithConfig).AssemblyQualifiedName }
                             });
            when_executing_command("-Configuration Debugx86");
        }

        [Test]
        public void configuration_is_not_set()
        {
            PackageBuilderWithConfig.ConfigurationValue.ShouldBe("Debugx86");
        }
    }
}