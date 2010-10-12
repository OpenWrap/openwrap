using System.Collections.Generic;

namespace OpenWrap.Dependencies
{
    public interface IDescriptorParser
    {
        void Parse(string line, PackageDescriptor descriptor);
        IEnumerable<string> Write(PackageDescriptor descriptor);
    }
}