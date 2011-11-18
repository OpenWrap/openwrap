using NUnit.Framework;
using OpenFileSystem.IO;
using Tests.Build.build_instruction_emitter.contexts;

namespace Tests.Build.build_instruction_emitter
{
    public class satellite_assembly_for_referenced_assemblies : msbuild_emitter
    {
        public satellite_assembly_for_referenced_assemblies()
        {
            given_export_name("bin-net35");
            given_output("sauron.dll");

            given_output("en", "sauron.resources.dll");

            when_generating_instructions();
        }

        [Test]
        public void included()
        {
            should_have_file(@"bin-net35\en", @"sauron.resources.dll");
        }
    }
}