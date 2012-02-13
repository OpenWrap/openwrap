using System.Reflection;
using NUnit.Framework;

namespace Tests.Build.assembly_info
{
    public class assembly_version : context_assembly_info
    {
        public assembly_version()
        {
            given_descriptor(
                "version: 1.0.0." + ushort.MaxValue,
                "assembly-info: assembly-version");
            when_generating_assembly_info();
        }
        [Test]
        public void contains_attribute()
        {
            should_have<AssemblyVersionAttribute>(
                "1.0.0.0");
        }
    }
}