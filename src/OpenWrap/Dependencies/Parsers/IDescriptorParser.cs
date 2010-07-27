namespace OpenWrap.Dependencies
{
    public interface IDescriptorParser
    {
        void Parse(string line, WrapDescriptor descriptor);
    }
}