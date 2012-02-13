using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using OpenWrap.PackageModel;

namespace OpenWrap.Repositories
{
    public class PreAuthenticatedRepository : IPackageRepository, ISupportAuthentication
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
            if (typeof(TFeature) == typeof(ISupportAuthentication)) return this as TFeature;
            return _remote.Feature<TFeature>();
        }

        readonly NetworkCredential _initialCredentials;
        IDisposable _credentialCookie;

        public PreAuthenticatedRepository(IPackageRepository remote, ISupportAuthentication auth, NetworkCredential initialCredentials)
        {
            _remote = remote;
            _auth = auth;
            CurrentCredentials = _initialCredentials = initialCredentials;
            _credentialCookie = auth.WithCredentials(initialCredentials);
        }

        public NetworkCredential CurrentCredentials { get; private set; }

        public IDisposable WithCredentials(NetworkCredential credentials)
        {
            // dispose the previous auth context. When coming back, reset to original credentials
            _credentialCookie.Dispose();

            CurrentCredentials = credentials;
            _credentialCookie = _auth.WithCredentials(credentials);
            return new ActionOnDispose(() =>
            {
                _credentialCookie.Dispose();
                CurrentCredentials = _initialCredentials;
                _credentialCookie = _auth.WithCredentials(_initialCredentials);
            });
        }
    }
}