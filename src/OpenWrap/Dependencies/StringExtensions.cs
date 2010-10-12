using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace OpenWrap.Dependencies
{
    public static class StringExtensions
    {
        static readonly Regex _foldableLines = new Regex(@"\r\n[\f\t\v\x85\p{Z}]+", RegexOptions.Multiline | RegexOptions.Compiled);
        public static string Join(this IEnumerable<string> strings, string separator)
        {
            return string.Join(separator, strings.ToArray());
        }
        public static string[] GetUnfoldedLines(this string content)
        {
            content = _foldableLines.Replace(content, " ");
            return content.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => x != string.Empty)
                    .ToArray();
        }
    }
}