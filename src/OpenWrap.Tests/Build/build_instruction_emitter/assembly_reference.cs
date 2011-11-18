using NUnit.Framework;
using OpenFileSystem.IO;
using Tests.Build.build_instruction_emitter.contexts;

namespace Tests.Build.build_instruction_emitter
{
    public class assembly_reference : msbuild_emitter
    {
        public assembly_reference()
        {
            given_export_name("bin-net35");
            given_output(".", "sauron.dll");
            when_generating_instructions();
        }

        [Test]
        public void exported()
        {
            should_have_file("bin-net35", "sauron.dll");
        }
    }
}