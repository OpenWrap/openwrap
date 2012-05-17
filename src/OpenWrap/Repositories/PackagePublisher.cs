using System;
using System.IO;
using OpenWrap.PackageModel;

namespace OpenWrap.Repositories
{
    public class PackagePublisher : IPackagePublisher, IPackagePublisherWithSource
    {
        readonly Action<IPackageRepository, string, Stream> _publish;
        readonly Action _end;

        public PackagePublisher(Action<IPackageRepository, string, Stream> publish, Action end = null)
        {
            _publish = publish;
            _end = end;
        }

        public void Dispose()
        {
            if (_end != null)
                _end();
        }

        public void Publish(string packageFileName, Stream packageStream)
        {
            _publish(null,packageFileName, packageStream);
        }

        public void Publish(IPackageRepository source, string packageFileName, Stream packageStream)
        {
            _publish(source, packageFileName, packageStream);
        }
    }
}