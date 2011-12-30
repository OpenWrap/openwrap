using NUnit.Framework;
using OpenWrap.contexts;
using OpenWrap.Testing;

namespace package_descriptor_specs
{
    public class specify_values_in_descriptor : descriptor
    {
        [Test]
        public void specify_runtime_assemblies()
        {
            given_descriptor("runtime-assemblies: Foo.dll, Bar.dll");

            Descriptor.RuntimeAssemblies.ShouldBe("Foo.dll, Bar.dll");
        }
    }
}