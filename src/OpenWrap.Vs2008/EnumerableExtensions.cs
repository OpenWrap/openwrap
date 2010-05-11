using System.Collections.Generic;
using EnvDTE;

namespace OpenWrap.Vs2008
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<AddIn> AsEnumerable(this AddIns addins)
        {
            foreach(AddIn addin in addins)
                yield return addin;
        }
    }
}