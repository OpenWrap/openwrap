using System.Collections.Generic;

namespace OpenWrap.Commands
{
    public class NamespaceNotFound : Error
    {
        readonly IEnumerable<string> _namespaceList;

        public NamespaceNotFound(IEnumerable<string> namespaceList)
        {
            _namespaceList = namespaceList;
        }

        public bool Success
        {
            get { return false; }
        }
    }
}