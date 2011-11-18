using NUnit.Framework;
using Tests.Build.build_instruction_emitter.contexts;

namespace Tests.Build.build_instruction_emitter
{
    public class output_assembly_with_bin_disabled : msbuild_emitter
    {
        public output_assembly_with_bin_disabled()
        {
            given_export_name("bin-net35");
            given_output("sauron.dll");
            given_includes(bin: false);
            when_generating_instructions();
        }

        [Test]
        public void is_exported()
        {
            should_not_have_file("bin-net35", "sauron.dll");
        }
    }
}