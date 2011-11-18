using NUnit.Framework;
using Tests.Build.build_instruction_emitter.contexts;

namespace Tests.Build.build_instruction_emitter
{
    public class openwrap_reference : msbuild_emitter
    {
        public openwrap_reference()
        {
            given_export_name("bin-net35");
            given_assembly_reference("sauron.dll");
            given_openwrap_reference("sauron.dll");
            when_generating_instructions();
        }

        [Test]
        public void not_exported()
        {
            should_not_have_file("bin-net35", "sauron.dll");
        }
    }
}