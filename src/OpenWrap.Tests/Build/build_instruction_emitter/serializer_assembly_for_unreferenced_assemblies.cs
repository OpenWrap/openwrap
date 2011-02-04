using NUnit.Framework;
using OpenWrap.Tests.Build.build_instruction_emitter_specs.contexts;

namespace OpenWrap.Build.build_instruction_emitter
{
    public class serializer_assembly_for_unreferenced_assemblies : msbuild_emitter
    {
        public serializer_assembly_for_unreferenced_assemblies()
        {
            given_export_name("bin-net35");

            given_serialization(@"sauron.XmlSerializers.dll");

            when_generating_instructions();
        }

        [Test]
        public void included()
        {
            should_not_have_file(@"bin-net35", @"sauron.XmlSerializers.dll");
        }
    }
}