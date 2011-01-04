using System.IO;

namespace OpenWrap.PackageModel
{
    public interface IPackageDescriptorEntry
    {
        string Name { get; }
        string Value { get; }
        void Write(TextWriter writer);
    }
}