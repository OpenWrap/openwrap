using System.IO;

namespace OpenWrap.Repositories
{
    public interface ISupportPublishing : IPackageRepository
    {
        IPackageInfo Publish(string packageFileName, Stream packageStream);
        void PublishCompleted();
    }
}