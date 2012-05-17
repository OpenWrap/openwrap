using System.Reflection;
using NUnit.Framework;

namespace Tests.Build.assembly_info
{
    public class copyright : context_assembly_info
    {
        public copyright()
        {
            given_descriptor(
                "copyright: Tolkien",
                "assembly-info: copyright");
            when_generating_assembly_info();
        }
        [Test]
        public void contains_attribute()
        {
            should_have<AssemblyCopyrightAttribute>("Tolkien");
        }
    }
}