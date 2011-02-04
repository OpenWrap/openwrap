using NUnit.Framework;
using OpenWrap.Tests.Build.build_instruction_emitter_specs.contexts;

namespace OpenWrap.Build.build_instruction_emitter
{
    public class assembly_reference : msbuild_emitter
    {
        public assembly_reference()
        {
            given_export_name("bin-net35");
            given_assembly_reference("sauron.dll");
            when_generating_instructions();
        }

        [Test]
        public void exported()
        {
            should_have_file("bin-net35", "sauron.dll");
        }
    }
}