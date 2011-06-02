using System;
using System.Collections.Generic;
using System.Text;

namespace OpenWrap.Configuration
{
    static class ConfigurationHelper
    {
        public static IEnumerable<KeyValuePair<string, string>> ParseKeyValuePairs(string input)
        {
            const int KEY = 0;
            const int BEFORE_VAL = 1;
            const int QVALUE = 2;
            const int NONE = -1;
            int state = NONE;
            string key = null;
            string value = null;
            var sb = new StringBuilder();

            char curChar = '\0';
            Action append = () => sb.Append(curChar);
            Action commitKey = () => { key = sb.ToString(); sb = new StringBuilder(); state = BEFORE_VAL; };
            Func<KeyValuePair<string,string>> commitVal = () => { value = sb.ToString(); sb = new StringBuilder(); state = NONE; return new KeyValuePair<string, string>(key,value);};

            for (int pos = 0; pos < input.Length; pos++)
            {
                curChar = input[pos];
                if (curChar == '=') { if (state == KEY) commitKey(); else if (state == QVALUE) append(); }
                else if (curChar == ';') { if (state == QVALUE) append(); }
                else if (curChar == '\\') { if (pos < input.Length-1) sb.Append(input[++pos]); }
                else if (curChar == '"')
                {
                    if (state == QVALUE) yield return commitVal();
                    else if (state == BEFORE_VAL) state = QVALUE;
                }
                else if (curChar == ' ')
                {
                    if (state == QVALUE) append();
                    else if (state == KEY) commitKey();
                }
                else
                {
                    if (state == NONE) state = KEY;
                    append();
                }
            }
        }
    }
}