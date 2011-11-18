using NUnit.Framework;
using Tests.Build.build_instruction_emitter.contexts;

namespace Tests.Build.build_instruction_emitter
{
    public class content_in_subfolder : msbuild_emitter
    {
        public content_in_subfolder()
        {
            given_export_name("bin-net35");
            given_content_file(@"rings\of\power\one-ring.cs");
            when_generating_instructions();
        }

        [Test]
        public void copied_in_subfolder_exports()
        {
            should_have_file(@"bin-net35\rings\of\power", @"rings\of\power\one-ring.cs");
        }
    }
}