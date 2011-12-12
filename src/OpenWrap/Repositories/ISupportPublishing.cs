using System;
using System.IO;
using OpenWrap.PackageModel;

namespace OpenWrap.Repositories
{
    public interface ISupportPublishing : IRepositoryFeature
    {
        IPackagePublisher Publisher();
    }
    public interface IPackagePublisher : IDisposable
    {
        void Publish(string packageFileName, Stream packageStream);        
    }
    public interface IPackagePublisherWithSource : IPackagePublisher
    {
        void Publish(IPackageRepository source, string packageFileName, Stream packageStream);
    }
}