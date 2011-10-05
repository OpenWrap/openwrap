using System;
using System.IO;
using OpenWrap.PackageModel;

namespace OpenWrap.Repositories
{
    public class PackagePublisher : IPackagePublisher, IPackagePublisherWithSource
    {
        readonly Func<IPackageRepository, string, Stream, IPackageInfo> _publish;
        readonly Action _end;

        public PackagePublisher(Func<IPackageRepository, string, Stream, IPackageInfo> publish, Action end = null)
        {
            _publish = publish;
            _end = end;
        }

        public void Dispose()
        {
            if (_end != null)
                _end();
        }

        public IPackageInfo Publish(string packageFileName, Stream packageStream)
        {
            return _publish(null,packageFileName, packageStream);
        }

        public IPackageInfo Publish(IPackageRepository source, string packageFileName, Stream packageStream)
        {
            return _publish(source, packageFileName, packageStream);
        }
    }
}