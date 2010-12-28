using System;
using NUnit.Framework;
using OpenWrap.Tests.Build.build_instruction_emitter_specs.contexts;

namespace OpenWrap.Tests.Build.build_instruction_emitter_specs
{
    public class output_assembly : msbuild_emitter
    {
        public output_assembly()
        {
            given_export("bin-net35");
            given_output_assembly("sauron.dll");
            when_generating_instructions();
        }

        [Test]
        public void is_exported()
        {
            should_have_file("bin-net35", "sauron.dll");
        }
    }
    public class assembly_reference : msbuild_emitter
    {
        public assembly_reference()
        {
            given_export("bin-net35");
            given_assembly_reference("sauron.dll");
            when_generating_instructions();
        }

        [Test]
        public void exported()
        {
            should_have_file("bin-net35", "sauron.dll");
        }
    }
    public class openwrap_reference : msbuild_emitter
    {
        public openwrap_reference()
        {
            given_export("bin-net35");
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
    public class pdb_for_referenced_assembly : msbuild_emitter
    {
        public pdb_for_referenced_assembly()
        {
            given_export("bin-net35");
            given_assembly_reference("sauron.dll");
            given_pdb("sauron.pdb");
            when_generating_instructions();
        }

        [Test]
        public void pdb_is_included()
        {
            should_have_file("bin-net35", "sauron.pdb");
        }
    }
    public class pdb_for_unreferenced_assembly : msbuild_emitter
    {
        public pdb_for_unreferenced_assembly()
        {
            given_export("bin-net35");
            given_pdb("sauron.pdb");
            when_generating_instructions();
        }

        [Test]
        public void not_included()
        {
            should_not_have_file("bin-net35", "sauron.pdb");
        }
    }
    public class pdb_for_openwrap_assembly : msbuild_emitter
    {
        public pdb_for_openwrap_assembly()
        {
            given_export("bin-net35");
            given_assembly_reference("sauron.dll");
            given_openwrap_reference("sauron.dll");
            given_pdb("sauron.pdb");

            when_generating_instructions();
        }

        [Test]
        public void not_included()
        {
            should_not_have_file("bin-net35", "sauron.pdb");
        }
    }
    public class doc_for_referenced_assembly : msbuild_emitter
    {
        public doc_for_referenced_assembly()
        {
            given_export("bin-net35");
            given_assembly_reference("sauron.dll");

            given_documentation_file("sauron.xml");
            when_generating_instructions();
        }

        [Test]
        public void included()
        {
            should_have_file("bin-net35", "sauron.xml");
        }
    }
    public class doc_for_openwrap_assembly : msbuild_emitter
    {
        public doc_for_openwrap_assembly()
        {
            given_export("bin-net35");
            given_assembly_reference("sauron.dll");
            given_openwrap_reference("sauron.dll");
            given_documentation_file("sauron.xml");

            when_generating_instructions();
        }

        [Test]
        public void included()
        {
            should_not_have_file("bin-net35", "sauron.xml");
        }
    }
    public class doc_for_unreferenced_assembly : msbuild_emitter
    {
        public doc_for_unreferenced_assembly()
        {
            given_export("bin-net35");

            given_documentation_file("sauron.xml");

            when_generating_instructions();
        }

        [Test]
        public void included()
        {
            should_not_have_file("bin-net35", "sauron.xml");
        }
    }
    public class satellite_assembly_for_referenced_assemblies : msbuild_emitter
    {
        public satellite_assembly_for_referenced_assemblies()
        {
            given_export("bin-net35");
            given_assembly_reference("sauron.dll");

            given_satellite(@"en\sauron.resources.dll");

            when_generating_instructions();
        }

        [Test]
        public void included()
        {
            should_have_file(@"bin-net35\en", @"en\sauron.resources.dll");
        }
    }
    public class satellite_assembly_for_unreferenced_assemblies : msbuild_emitter
    {
        public satellite_assembly_for_unreferenced_assemblies()
        {
            given_export("bin-net35");

            given_satellite(@"en\sauron.resources.dll");

            when_generating_instructions();
        }

        [Test]
        public void included()
        {
            should_not_have_file(@"bin-net35\en", @"en\sauron.resources.dll");
        }
    }
    public class satellite_assembly_for_openwrap_assemblies : msbuild_emitter
    {
        public satellite_assembly_for_openwrap_assemblies()
        {
            given_export("bin-net35");

            given_assembly_reference("sauron.dll");
            given_openwrap_reference("sauron.dll");

            given_satellite(@"en\sauron.resources.dll");

            when_generating_instructions();
        }

        [Test]
        public void included()
        {
            should_not_have_file(@"bin-net35\en", @"en\sauron.resources.dll");
        }
    }

    public class serializer_assembly_for_referenced_assemblies : msbuild_emitter
    {
        public serializer_assembly_for_referenced_assemblies()
        {
            given_export("bin-net35");
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
    public class serializer_assembly_for_unreferenced_assemblies : msbuild_emitter
    {
        public serializer_assembly_for_unreferenced_assemblies()
        {
            given_export("bin-net35");

            given_serialization(@"sauron.XmlSerializers.dll");

            when_generating_instructions();
        }

        [Test]
        public void included()
        {
            should_not_have_file(@"bin-net35", @"sauron.XmlSerializers.dll");
        }
    }
    public class serializer_assembly_for_openwrap_assemblies : msbuild_emitter
    {
        public serializer_assembly_for_openwrap_assemblies()
        {
            given_export("bin-net35");

            given_assembly_reference("sauron.dll");
            given_openwrap_reference("sauron.dll");

            given_serialization(@"sauron.XmlSerializers.dll");

            when_generating_instructions();
        }

        [Test]
        public void included()
        {
            should_not_have_file(@"bin-net35", @"sauron.XmlSerializers.dll");
        }
    }
    public class content : msbuild_emitter
    {
        public content()
        {
            given_export("bin-net35");
            given_content_file("one-ring.cs");
            when_generating_instructions();
        }

        [Test]
        public void exported()
        {
            should_have_file("bin-net35", "one-ring.cs");
        }
    }
    public class content_in_subfolder : msbuild_emitter
    {
        public content_in_subfolder()
        {
            given_export("bin-net35");
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