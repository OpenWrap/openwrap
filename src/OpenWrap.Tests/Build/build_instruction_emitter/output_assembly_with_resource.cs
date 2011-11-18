using NUnit.Framework;
using Tests.Build.build_instruction_emitter.contexts;

namespace Tests.Build.build_instruction_emitter
{
    public class output_assembly_with_resource : msbuild_emitter
    {
        public output_assembly_with_resource()
        {
            given_export_name("bin-net35");
            given_output_assembly("sauron.dll");
            given_satellite("sauron.resources.dll");
            when_generating_instructions();
        }

        [Test]
        public void resource_is_exported()
        {
            should_have_file("bin-net35", "sauron.resources.dll");
        }

        [Test]
        public void assembly_is_exported()
        {
            should_have_file("bin-net35", "sauron.dll");
        }
    }
}