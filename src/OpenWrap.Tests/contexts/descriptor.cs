using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenWrap;
using OpenWrap.IO;
using OpenWrap.PackageModel;
using OpenWrap.PackageModel.Serialization;
using OpenWrap.Testing;

namespace Tests.contexts
{
    public abstract class descriptor : context
    {
        protected PackageDescriptor Descriptor;
        protected string DescriptorContent;

        protected virtual void when_writing()
        {
            DescriptorContent = WriteDescriptor(Descriptor);
        }

        protected static string WriteDescriptor(IPackageDescriptor descriptor)
        {
            var memString = new MemoryStream();
            new PackageDescriptorWriter().Write(descriptor.GetPersistableEntries(), memString);
            memString.Position = 0;
            return memString.ReadString();
        }

        protected void descriptor_should_be(params string[] expectedContent)
        {
            var joinedContent = expectedContent.JoinString("\r\n");
            if (!string.IsNullOrEmpty(joinedContent))
                joinedContent += "\r\n";
            DescriptorContent.ShouldBe(joinedContent);
        }
        protected void given_descriptor(params string[] lines)
        {
            Descriptor = ReadDescriptor(lines, _=>new PackageDescriptor(_));
        }

        protected T ReadDescriptor<T>(string[] lines, Func<IEnumerable<IPackageDescriptorEntry>, T> ctor)
            where T : class, IPackageDescriptor
        {
            var lineChars = string.Join("\r\n", lines);
            return new PackageDescriptorReader().Read(
                new MemoryStream(Encoding.UTF8.GetBytes(lineChars)),
                ctor);
        }
    }
}