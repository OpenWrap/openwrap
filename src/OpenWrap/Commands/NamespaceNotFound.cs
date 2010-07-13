using System.Collections.Generic;
using System.Linq;

namespace OpenWrap.Commands
{
    public class NamespaceNotFound : Error
    {
        readonly string _message;

        public NamespaceNotFound(IEnumerable<string> namespaceList)
        {
            var names = namespaceList.ToArray();
            if (names.Length == 0)
                _message = "Namespace not found.";
            else
                _message = "Ambiguous namespace. Possible matches: " + string.Join(", ", names);
        }

        public bool Success
        {
            get { return false; }
        }

        public override string ToString()
        {
            return _message;
        }
    }
}