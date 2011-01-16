using System.Collections.Generic;

namespace OpenWrap.IO
{
    public abstract class PathSegment
    {
        public abstract bool TryParse(IDictionary<string, string> properties, LinkedListNode<PathSegment> currentParser, ref LinkedListNode<string> currentSegment);
    }
}