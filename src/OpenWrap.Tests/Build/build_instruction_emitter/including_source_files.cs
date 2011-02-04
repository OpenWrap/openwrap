using NUnit.Framework;
using OpenWrap.Tests.Build.build_instruction_emitter_specs.contexts;

namespace OpenWrap.Build.build_instruction_emitter
{
    public class including_source_files : msbuild_emitter
    {
        public including_source_files()
        {
            given_base_path(@"c:\temp");
            given_export_name("bin-net35");
            given_includes(source: true);
            given_source_file(@"relative\path\relativesource.cs");
            given_source_file(@"c:\temp\absolute\path\absolutesource.cs");

            when_generating_instructions();
        }

        [Test]
        public void relative_is_copied_in_subfolder()
        {
            should_have_file(@"source\relative\path", @"relative\path\relativesource.cs");
        }
        [Test]
        public void absolute_is_copied_in_subfolder()
        {
            should_have_file(@"source\absolute\path", @"absolute\path\absolutesource.cs");
        }
    }
}