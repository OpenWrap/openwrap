using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace OpenWrap.PackageModel.Serialization
{
    public static class StringExtensions
    {
        static readonly Regex _foldableLines = new Regex(@"\r\n[\f\t\v\x85\p{Z}]+", RegexOptions.Multiline | RegexOptions.Compiled);

        public static string[] GetUnfoldedLines(this string content)
        {
            content = _foldableLines.Replace(content, " ");
            var linesToProcess = content.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            return linesToProcess
                    .Select(x => x.Trim())
                    .Where(x => x != string.Empty)
                    .ToArray();
        }
    }
}