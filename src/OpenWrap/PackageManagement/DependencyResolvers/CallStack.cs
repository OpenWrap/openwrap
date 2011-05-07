using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace OpenWrap.PackageManagement.DependencyResolvers
{
    public class CallStack : IEnumerable<Node>
    {
        readonly ReadOnlyCollection<Node> _nodes;

        public CallStack(IEnumerable<Node> node)
        {
            var nodes = node.ToList();
            nodes.Reverse();
            _nodes = nodes.AsReadOnly();
        }

        public override string ToString()
        {
            return _nodes.Select(x => x.ToString()).JoinString(" -> ");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<Node> GetEnumerator()
        {
            return _nodes.GetEnumerator();
        }
    }
}