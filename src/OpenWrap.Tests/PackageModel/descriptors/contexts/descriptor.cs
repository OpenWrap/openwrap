using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenWrap.IO;
using OpenWrap.PackageModel.Serialization;
using OpenWrap.Testing;

namespace OpenWrap.PackageModel.descriptors.contexts
{
    public abstract class descriptor : context
    {
        protected PackageDescriptor Descriptor;
        protected string DescriptorContent;

        protected virtual void when_writing()
        {
            DescriptorContent = WriteDescriptor(Descriptor);
        }

        protected string WriteDescriptor(IPackageDescriptor descriptor)
        {
            var memString = new MemoryStream();
            new PackageDescriptorReaderWriter().Write(descriptor, memString);
            memString.Position = 0;
            return memString.ReadString();
        }

        protected void descriptor_should_be(params string[] expectedContent)
        {
            var joinedContent = expectedContent.Join("\r\n");
            if (!string.IsNullOrEmpty(joinedContent))
                joinedContent += "\r\n";
            DescriptorContent.ShouldBe(joinedContent);
        }
        protected void given_descriptor(params string[] lines)
        {
            Descriptor = ReadDescriptor<PackageDescriptor>(lines);
        }

        protected T ReadDescriptor<T>(string[] lines, Func<IEnumerable<IPackageDescriptorEntry>, T> ctor = null)
            where T : class, IPackageDescriptor
        {
            var lineChars = string.Join("\r\n", lines);
            return new PackageDescriptorReaderWriter().Read(
                new MemoryStream(Encoding.UTF8.GetBytes(lineChars)),
                ctor);
        }
    }
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
            ScopedDescriptor = ReadDescriptor(lines, x=>new PackageDescriptor.ScopedPackageDescriptor(Descriptor, x));
        }
        protected void scoped_descriptor_should_be(params string[] expectedContent)
        {
            var joinedContent = expectedContent.Join("\r\n");
            if (!string.IsNullOrEmpty(joinedContent))
                joinedContent += "\r\n";
            ScopedDescriptorContent.ShouldBe(joinedContent);
        }
    }
}