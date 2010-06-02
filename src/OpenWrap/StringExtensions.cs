using System.Text.RegularExpressions;
namespace OpenWrap
{
    public static class StringExtensions
    {
        public static string CamelToSpacedName(this string str)
        {
            return caseChange.Replace(str, "$1 $2");
        }

        static readonly Regex caseChange = new Regex("([a-z])([A-Z])");
    }
}