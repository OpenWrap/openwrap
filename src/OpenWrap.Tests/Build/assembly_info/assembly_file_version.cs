using System.Reflection;
using NUnit.Framework;

namespace Tests.Build.assembly_info
{
    public class assembly_file_version : context_assembly_info
    {
        public assembly_file_version()
        {
            given_descriptor(
                "version: 1.0.0." + ushort.MaxValue,
                "assembly-info: file-version");
            when_generating_assembly_info();
        }
        [Test]
        public void contains_attribute()
        {
            should_have<AssemblyFileVersionAttribute>(
                "1.0.0.65535");
        }
    }
}