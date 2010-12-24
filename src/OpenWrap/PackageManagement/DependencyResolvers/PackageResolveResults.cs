using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Collections;
using OpenWrap.PackageModel;

namespace OpenWrap.PackageManagement.DependencyResolvers
{
    public class PackageResolveResults : IEnumerable<KeyValuePair<PackageIdentifier, PackageResolutionStacks>>
    {
        readonly Dictionary<PackageIdentifier, PackageResolutionStacks> _nodes;
        readonly PackageResolveResults _source;

        public PackageResolveResults()
        {
            _nodes = new Dictionary<PackageIdentifier, PackageResolutionStacks>();
        }

        public PackageResolveResults(PackageResolveResults source)
        {
            _source = source;
            _nodes = source._nodes.ToDictionary(x => x.Key, x => new PackageResolutionStacks(x.Value.Successful, x.Value.Failed));
        }

        public PackageResolutionStacks this[PackageIdentifier id]
        {
            get
            {
                if (id == null) throw new ArgumentNullException("id");
                return _nodes.Get(id) ?? PackageResolutionStacks.Null;
            }
        }

        public void Add(PackageIdentifier identifier,
                        IEnumerable<CallStack> successful = null,
                        IEnumerable<CallStack> failing = null)
        {
            if (identifier == null) throw new ArgumentNullException("identifier");
            successful = successful.EmptyIfNull();
            failing = failing.EmptyIfNull();

            _nodes[identifier] = _nodes.GetOrCreate(identifier).Combine(successful, failing);
        }

        public PackageIdentifier FromName(string name)
        {
            if (name == null) throw new ArgumentNullException("name");
            return _nodes.Keys.FirstOrDefault(x => x.Name.EqualsNoCase(name));
        }

        public void Remove(PackageIdentifier identifier)
        {
            if (identifier == null) throw new ArgumentNullException("identifier");
            if (_nodes.ContainsKey(identifier))
                _nodes.Remove(identifier);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<PackageIdentifier, PackageResolutionStacks>> GetEnumerator()
        {
            foreach (var kv in _nodes)
                if (kv.Value != PackageResolutionStacks.Null)
                    yield return kv;
        }
    }
}