using NUnit.Framework;
using Tests.Build.build_instruction_emitter.contexts;

namespace Tests.Build.build_instruction_emitter
{
    public class satellite_assembly_for_openwrap_assemblies : msbuild_emitter
    {
        public satellite_assembly_for_openwrap_assemblies()
        {
            given_export_name("bin-net35");

            given_assembly_reference("sauron.dll");
            given_openwrap_reference("sauron.dll");

            given_satellite(@"en\sauron.resources.dll");

            when_generating_instructions();
        }

        [Test]
        public void included()
        {
            should_not_have_file(@"bin-net35\en", @"en\sauron.resources.dll");
        }
    }
}