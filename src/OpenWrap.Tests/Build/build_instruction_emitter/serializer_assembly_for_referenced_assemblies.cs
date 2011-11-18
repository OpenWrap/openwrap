using NUnit.Framework;
using Tests.Build.build_instruction_emitter.contexts;

namespace Tests.Build.build_instruction_emitter
{
    public class serializer_assembly_for_referenced_assemblies : msbuild_emitter
    {
        public serializer_assembly_for_referenced_assemblies()
        {
            given_export_name("bin-net35");
            given_assembly_reference("sauron.dll");

            given_serialization(@"sauron.XmlSerializers.dll");

            when_generating_instructions();
        }

        [Test]
        public void included()
        {
            should_have_file(@"bin-net35", @"sauron.XmlSerializers.dll");
        }
    }
}