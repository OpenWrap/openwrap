using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace OpenWrap.Repositories
{
    public class CallStack : IEnumerable<Node>
    {
        ReadOnlyCollection<Node> _nodes;

        public CallStack(IEnumerable<Node> node)
        {
            var nodes = node.ToList();
            nodes.Reverse();
            _nodes = nodes.AsReadOnly();
        }

        public IEnumerator<Node> GetEnumerator()
        {
            return _nodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public override string ToString()
        {
            return _nodes.Select(x => x.ToString()).Join(" -> ");
        }
    }
}