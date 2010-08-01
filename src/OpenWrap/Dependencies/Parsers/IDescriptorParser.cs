using System.Collections.Generic;

namespace OpenWrap.Dependencies
{
    public interface IDescriptorParser
    {
        void Parse(string line, WrapDescriptor descriptor);
        IEnumerable<string> Write(WrapDescriptor descriptor);
    }
}