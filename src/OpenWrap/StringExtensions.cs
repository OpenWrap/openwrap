using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public static string GetCamelCaseInitials(this string str)
        {
            return new string(str.Where(char.IsUpper).ToArray());
        }
        public static IEnumerable<string> SplitCamelCase(this string str)
        {
            StringBuilder currentValue = new StringBuilder();
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
        public static bool MatchesHumps(this string name, string value)
        {
            return name.ToUpperInvariant().MatchesHumps(0, value.ToUpperInvariant(), 0);
        }


        static bool MatchesHumps(this string name, int namePosition, string value, int valuePosition)
        {
            if (valuePosition > value.Length-1) return false;
            var foundPosition = value.IndexOf(name[namePosition], valuePosition);
            return foundPosition != -1 &&
                    (namePosition == name.Length-1 ||
                     MatchesHumps(name, namePosition + 1, value, foundPosition + 1));
        }

        static bool CharEq(int namePosition, string name, int valuePosition, string value)
        {
            return char.ToUpperInvariant(value[valuePosition]) == char.ToUpperInvariant(name[namePosition]);
        }
        public static Version ToVersion(this string version)
        {
            try
            {
                return new Version(version);
            }catch
            {
                return null;
            }
        }
    }
}