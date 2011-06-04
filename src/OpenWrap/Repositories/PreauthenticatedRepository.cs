using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using OpenWrap.PackageModel;

namespace OpenWrap.Repositories
{
    public class PreauthenticatedRepository : IPackageRepository, ISupportAuthentication
    {
        readonly IPackageRepository _remote;
        public string Type
        {
            get { return _remote.Type; }
        }

        readonly ISupportAuthentication _auth;

        public ILookup<string, IPackageInfo> PackagesByName
        {
            get { return _remote.PackagesByName; }
        }

        public IEnumerable<IPackageInfo> FindAll(PackageDependency dependency)
        {
            return _remote.FindAll(dependency);
        }

        public void RefreshPackages()
        {
            _remote.RefreshPackages();
        }

        public string Name
        {
            get { return _remote.Name; }
        }

        public string Token
        {
            get { return _remote.Token; }
        }

        public TFeature Feature<TFeature>() where TFeature : class, IRepositoryFeature
        {
            return _remote.Feature<TFeature>();
        }

        readonly NetworkCredential _credentials;
        IDisposable _credentialCookie;

        public PreauthenticatedRepository(IPackageRepository remote, ISupportAuthentication auth, NetworkCredential credentials)
        {
            _remote = remote;
            _auth = auth;
            _credentials = credentials;
            _credentialCookie = auth.WithCredentials(credentials);
        }

        public IDisposable WithCredentials(NetworkCredential credentials)
        {
            // dispose the previous auth context. When coming back, reset to original credentials
            _credentialCookie.Dispose();
            _credentialCookie = _auth.WithCredentials(credentials);
            return new ActionOnDispose(() =>
            {
                _credentialCookie.Dispose();
                _credentialCookie = _auth.WithCredentials(_credentials);
            });
        }
    }
}