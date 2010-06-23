namespace OpenWrap.Dependencies
{
    public interface IWrapDescriptorLineParser
    {
        void Parse(string line, WrapDescriptor descriptor);
    }
}