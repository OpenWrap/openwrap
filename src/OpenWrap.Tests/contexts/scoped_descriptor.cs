using OpenWrap.PackageModel;
using OpenWrap.Testing;

namespace OpenWrap.contexts
{
    public abstract class scoped_descriptor : descriptor
    {
        protected IPackageDescriptor ScopedDescriptor;
        protected string ScopedDescriptorContent;

        protected override void when_writing()
        {
            base.when_writing();
            ScopedDescriptorContent = WriteDescriptor(ScopedDescriptor);
        }
        protected void given_scoped_descriptor(params string[] lines)
        {
            ScopedDescriptor = ReadDescriptor(lines, x=>Descriptor.CreateScoped(x));
        }
        protected void scoped_descriptor_should_be(params string[] expectedContent)
        {
            var joinedContent = expectedContent.JoinString("\r\n");
            if (!string.IsNullOrEmpty(joinedContent))
                joinedContent += "\r\n";
            ScopedDescriptorContent.ShouldBe(joinedContent);
        }
    }
}