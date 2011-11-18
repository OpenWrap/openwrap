using NUnit.Framework;
using Tests.Build.build_instruction_emitter.contexts;

namespace Tests.Build.build_instruction_emitter
{
    public class doc_for_referenced_assembly : msbuild_emitter
    {
        public doc_for_referenced_assembly()
        {
            given_export_name("bin-net35");
            given_output(".", "sauron.dll");

            given_output(".", "sauron.xml");
            when_generating_instructions();
        }

        [Test]
        public void included()
        {
            should_have_file("bin-net35", "sauron.xml");
        }
    }
}