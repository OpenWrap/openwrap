using System.Collections.Generic;
using System.Linq;

namespace OpenWrap.Commands
{
    public class NounNotFound : Error
    {
        readonly string _message;

        public NounNotFound(IEnumerable<string> nouns)
        {
            nouns = nouns.ToList();
            if (nouns.Count() == 0)
                _message = "Noun not found.";
            else
                _message = "Ambiguous noun. Possible matches: " + nouns.JoinString(", ");
        }

        public override string ToString()
        {
            return _message;
        }
    }
}