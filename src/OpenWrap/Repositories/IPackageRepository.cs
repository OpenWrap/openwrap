using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.PackageModel;

namespace OpenWrap.Repositories
{
    public interface IPackageRepository
    {
        ILookup<string, IPackageInfo> PackagesByName { get; }

        void RefreshPackages();
        string Name { get; }
        string Token { get; }
        string Type { get; }

        TFeature Feature<TFeature>() where TFeature : class, IRepositoryFeature;
    }
    public class NoPreReleasePackageRepository : IPackageRepository
    {
        readonly IPackageRepository _wrapped;
        public ILookup<string, IPackageInfo> PackagesByName { get; private set; }

        public void RefreshPackages()
        {
            _wrapped.RefreshPackages();
        }

        public string Name
        {
            get { return _wrapped.Name; }
        }

        public string Token
        {
            get { return _wrapped.Token; }
        }

        public string Type
        {
            get { return _wrapped.Type; }
        }

        public TFeature Feature<TFeature>() where TFeature : class, IRepositoryFeature
        {
            return _wrapped.Feature<TFeature>();
        }

        public NoPreReleasePackageRepository(IPackageRepository wrapped)
        {
            _wrapped = wrapped;
            PackagesByName = new LookupWrapper(wrapped.PackagesByName);

        }
        class LookupWrapper : ILookup<string,IPackageInfo>
        {
            readonly ILookup<string, IPackageInfo> _wrapped;
            public IEnumerator<IGrouping<string, IPackageInfo>> GetEnumerator()
            {
                foreach(var group in _wrapped.Select(_ => new GroupWrapper(_)))
                    yield return group;
            }

            public bool Contains(string key)
            {
                return _wrapped.Contains(key);
            }

            public int Count
            {
                get { return _wrapped.Count; }
            }

            public IEnumerable<IPackageInfo> this[string key]
            {
                get { return _wrapped[key].Where(_=>_.SemanticVersion.PreRelease == null); }
            }

            public LookupWrapper(ILookup<string,IPackageInfo> wrapped)
            {
                _wrapped = wrapped;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
        class GroupWrapper : IGrouping<string,IPackageInfo>
        {
            readonly IGrouping<string, IPackageInfo> _wrapped;

            public GroupWrapper(IGrouping<string,IPackageInfo> wrapped)
            {
                _wrapped = wrapped;
            }

            public IEnumerator<IPackageInfo> GetEnumerator()
            {
                foreach (var package in _wrapped.Where(_ => _.SemanticVersion.PreRelease == null))
                    yield return package;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public string Key
            {
                get { return _wrapped.Key; }
            }
        }
    }
}