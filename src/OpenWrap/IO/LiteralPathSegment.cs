using System.Collections.Generic;

namespace OpenWrap.IO
{
    public class LiteralPathSegment : PathSegment
    {
        readonly string _path;

        public LiteralPathSegment(string path)
        {
            _path = path;
        }

        public override bool TryParse(IDictionary<string, string> properties, LinkedListNode<PathSegment> currentParser, ref LinkedListNode<string> currentSegment)
        {
            bool success = currentSegment != null && currentSegment.Value.EqualsNoCase(_path);
            if (success)
                currentSegment = currentSegment.Next;
            return success;
        }
    }
}