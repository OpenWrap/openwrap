using System;
using System.Collections.Generic;

namespace OpenWrap.IO
{
    public class WildcardPathSegment : PathSegment
    {
        public override bool TryParse(IDictionary<string, string> properties, LinkedListNode<PathSegment> currentParser, ref LinkedListNode<string> currentSegment)
        {
            if (currentParser.Next == null)
                throw new ArgumentException("A wildcard segment cannot be at the end of a path.");

            var emptyProperties = new Dictionary<string, string>();
            var nextParser = currentParser.Next;
            var pathNode = currentSegment;

            while(pathNode != null)
            {
                var localPathNode = pathNode;
                var success = nextParser.Value.TryParse(emptyProperties, nextParser, ref localPathNode);
                if (!success)
                {
                    pathNode = pathNode.Next;
                }
                else
                {
                    currentSegment = pathNode;
                    return true;
                }
            }
            return false;
        }
    }
}