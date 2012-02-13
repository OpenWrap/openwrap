using System.Reflection;
using NUnit.Framework;

namespace Tests.Build.assembly_info
{
    public class title : context_assembly_info
    {
        public title()
        {
            given_descriptor(
                "title: The lord of the rings",
                "assembly-info: title");
            when_generating_assembly_info();
        }
        [Test]
        public void contains_attribute()
        {
            should_have<AssemblyProductAttribute>("The lord of the rings");
        }
    }
}