using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenWrap
{
    public static class StringExtensions
    {
        public static string CamelToSpacedName(this string str)
        {
            var builder = new StringBuilder();

            for (var i = 0; i < str.Length; i++)
            {
                var chr = str[i];

                if (str[i] >= 65 && str[i] <= 90)
                {
                    if (i > 0)
                    {
                        builder.Append(' ');
                        chr = (char)(chr + 32);
                    }
                }

                builder.Append(chr);
            }

            return builder.ToString();
        }
        public static bool ContainsNoCase(this string value, string valueToSearch)
        {
            return value.IndexOf(valueToSearch, StringComparison.OrdinalIgnoreCase) != -1;
        }

        public static bool ContainsNoCase(this IEnumerable<string> values, string valueToSearch)
        {
            return values.Contains(valueToSearch, StringComparer.OrdinalIgnoreCase);
        }

        public static bool EndsWithNoCase(this string value, string valueToCompare)
        {
            return value.EndsWith(valueToCompare, StringComparison.OrdinalIgnoreCase);
        }

        public static bool EqualsNoCase(this string value, string valueToCompare)
        {
            if (value == null) return valueToCompare == null;
            return value.Equals(valueToCompare, StringComparison.OrdinalIgnoreCase);
        }

        public static string GetCamelCaseInitials(this string str)
        {
            return new string(str.Where(char.IsUpper).ToArray());
        }

        public static string JoinString(this IEnumerable<string> strings, string separator)
        {
            return string.Join(separator, strings.ToArray());
        }

        public static string JoinString<T>(this IEnumerable<T> strings, string separator)
        {
            return string.Join(separator, strings.Select(_=>_.ToString()).ToArray());
        }
        public static string JoinString(this IEnumerable<string> strings, char separator)
        {
            return string.Join(separator + string.Empty, strings.ToArray());
        }
        public static bool MatchesHumps(this string name, string value)
        {
            var nameInv = name.ToUpperInvariant();
            var valueInv = value.ToUpperInvariant();
            if (nameInv[0] != valueInv[0]) return false;
            return nameInv.MatchesHumps(0, valueInv, 0);
        }
        public static IEnumerable<string> SelectHumps(this string key, IEnumerable<string> values)
        {
            return values.Where(x => key.MatchesHumps(x));
        }
        public static IEnumerable<string> SplitCamelCase(this string str)
        {
            var currentValue = new StringBuilder();
            for (int i = 0; i < str.Length; i++)
            {
                if (i == 0 || !char.IsUpper(str[i]))
                    currentValue.Append(str[i]);
                else if (char.IsUpper(str[i]))
                {
                    yield return currentValue.ToString();
                    currentValue = new StringBuilder().Append(str[i]);
                }
            }
            if (currentValue.Length > 0)
                yield return currentValue.ToString();
        }

        public static bool StartsWithNoCase(this string value, string valueToCompare)
        {
            return value.StartsWith(valueToCompare, StringComparison.OrdinalIgnoreCase);
        }


        public static Stream ToUTF8Stream(this string value)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(value));
        }

        public static Version ToVersion(this string version)
        {
            if (string.IsNullOrEmpty(version) || !Regex.IsMatch(version, @"\d+\.\d+(\.\d+(\.\d+)?)?"))
                return null;
            try
            {
                return new Version(version);
            }
            catch
            {
                return null;
            }
        }

        public static Regex Wildcard(this string stringValue, bool autoWrap = false)
        {
            if (autoWrap && !stringValue.Contains('?') && !stringValue.Contains('*'))
                stringValue = "*" + stringValue + "*";
            stringValue = Regex.Escape(stringValue).Replace("\\?", ".?").Replace("\\*", ".*");
            return new Regex("^" + stringValue + "$", RegexOptions.IgnoreCase);
        }

        public static string TrimNullSafe(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value.Trim();
        }
        static bool MatchesHumps(this string name, int namePosition, string value, int valuePosition)
        {
            if (valuePosition > value.Length - 1) return false;
            var foundPosition = value.IndexOf(name[namePosition], valuePosition);
            return foundPosition != -1 &&
                   (namePosition == name.Length - 1 ||
                    MatchesHumps(name, namePosition + 1, value, foundPosition + 1));
        }
    }
}