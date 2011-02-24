using System;
using System.Collections.Generic;

namespace OpenWrap.IO
{
    public class WildcardPathSegment : PathSegment
    {
        public override bool TryParse(IDictionary<string, string> properties, LinkedListNode<PathSegment> currentParser, ref LinkedListNode<string> currentSegment)
        {
            //if (currentParser.Next == null)
            //    throw new ArgumentException("A wildcard segment cannot be at the end of a path.");

            var emptyProperties = new Dictionary<string, string>();
            var nextParser = currentParser.Next;
            var parsedSegment = currentSegment;

            while(nextParser != null && parsedSegment != null)
            {
                var parsedSegmentCopy = parsedSegment;
                var success = nextParser.Value.TryParse(emptyProperties, nextParser, ref parsedSegmentCopy);
                if (success)
                {
                    currentSegment = parsedSegment;
                    return true;
                }
                parsedSegment = parsedSegment.Next;
            }
            if (nextParser == null) currentSegment = null;
            return nextParser == null; // reached the end
        }
    }
}