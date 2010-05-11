using System.Collections.Generic;

namespace OpenRasta.Wrap.Console
{
    public class NamesapceNotFound : Error
    {
        readonly IEnumerable<string> _namespaceList;

        public NamesapceNotFound(IEnumerable<string> namespaceList)
        {
            _namespaceList = namespaceList;
        }

        public bool Success
        {
            get { return false; }
        }
    }
}