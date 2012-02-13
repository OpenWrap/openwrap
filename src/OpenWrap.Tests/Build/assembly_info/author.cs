using System.Reflection;
using NUnit.Framework;

namespace Tests.Build.assembly_info
{
    public class author : context_assembly_info
    {
        public author()
        {
            given_descriptor(
                "author: sauron <sauron@middle.earth>",
                "author: frodo <frodo@middle.earth>",
                "assembly-info: author");
            when_generating_assembly_info();
        }
        [Test]
        public void contains_attribute()
        {
            should_have<AssemblyCompanyAttribute>(
                "sauron <sauron@middle.earth>, frodo <frodo@middle.earth>");
        }
    }
}