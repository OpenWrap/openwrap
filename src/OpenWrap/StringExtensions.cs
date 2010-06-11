using System.Linq;
using System.Text;

namespace OpenWrap
{
    public static class StringExtensions
    {
        public static string CamelToSpacedName(this string str)
        {
            var builder = new StringBuilder();

            for(var i = 0 ; i <str.Length;i++)
            {
                var chr = str[i];

                if (str[i] >= 65 && str[i] <= 90)
                {
                    if(i > 0)
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
    }
}