using NUnit.Framework;
using Tests.Build.build_instruction_emitter.contexts;

namespace Tests.Build.build_instruction_emitter
{
    public class output_assembly_with_pdb : msbuild_emitter
    {
        public output_assembly_with_pdb()
        {
            given_export_name("bin-net35");
            given_output_assembly("sauron.dll");
            given_pdb("sauron.pdb");
            when_generating_instructions();
        }

        [Test]
        public void pdb_is_exported()
        {
            should_have_file("bin-net35", "sauron.pdb");
        }

        [Test]
        public void assembly_is_exported()
        {
            should_have_file("bin-net35", "sauron.dll");            
        }
    }
}