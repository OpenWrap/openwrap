using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenWrap.Commands.Cli.Parser
{
    public class InputParser
    {
        const char ESCAPE = '`';
        const char DBL_QUOTE = '"';
        const char SINGLE_QUOTE = '\'';
        public IEnumerable<Input> Parse(string value)
        {
            ParseState state = ParseState.None;
            var buffer = new StringBuilder();
            string currentName = string.Empty;
            List<string> currentValues = new List<string>();
            List<Input> inputs = new List<Input>();
            char currentQuote = '\0';
            Action commitName = () =>
            {
                currentName = buffer.ToString();
                buffer = new StringBuilder();
                state = ParseState.BeforeValue;
            };
            Action commitValue = () =>
            {
                if (buffer.Length > 0)
                    currentValues.Add(buffer.ToString());
                buffer = new StringBuilder();
                state = ParseState.AfterValue;
            };
            Action commitInput = () =>
            {
                if ((currentValues.Count == 0 && currentName == string.Empty) == false)
                {
                    if (currentValues.Count == 0)
                        inputs.Add(new SingleValueInput { Name = currentName, Value = string.Empty });
                    else if (currentValues.Count == 1)
                        inputs.Add(new SingleValueInput { Name = currentName, Value = currentValues[0] });
                    else
                        inputs.Add(new MultiValueInput { Name = currentName, Values = currentValues.ToList() });
                }
                currentName = string.Empty;
                state = ParseState.None;
                currentValues.Clear();
            };
            Action<char> appendCharacter = character =>
            {

                if (state == ParseState.BeforeValue || state == ParseState.None)
                    state = ParseState.Value;
                if (state == ParseState.AfterValue)
                {
                    commitValue();
                    commitInput();
                    state = ParseState.Value;
                }
                buffer.Append(character);

            };
            for (var position = 0; position < value.Length; position++)
            {
                var current = value[position];
                
                if (current == DBL_QUOTE || current == SINGLE_QUOTE)
                {
                    if (state == ParseState.BeforeValue || state == ParseState.None)
                    {
                        currentQuote = current;
                        state = ParseState.QuotedValue;
                        continue;
                    }
                    if (state == ParseState.QuotedValue && current == currentQuote)
                    {
                        currentQuote = '\0';
                        commitValue();
                        continue;
                    }
                }
                if (current == '`')
                {
                    if (value.Length - (position + 1) == 0) throw new InputParserException("Incomplete command. The escape character '`' is at the end of the line.");

                    var c = value[++position];
                    appendCharacter(ConvertToSpecialCharacter(c));
                    
                    continue;
                }
                if (current == '-' && state == ParseState.None)
                {
                    state = ParseState.Name;
                    continue;
                }
                
                if (current == '-' && (state == ParseState.AfterValue || state ==ParseState.BeforeValue))
                {
                    commitInput();
                    state = ParseState.Name;
                    continue;
                }
                if (current == ',' && (state == ParseState.Value || state == ParseState.AfterValue))
                {
                    commitValue();
                    state = ParseState.BeforeValue;
                    continue;
                }
                if (char.IsWhiteSpace(current))
                {
                    if (state == ParseState.Name)
                    {
                        commitName();
                        state = ParseState.BeforeValue;
                    }
                    else if (state == ParseState.Value)
                    {
                        state = ParseState.AfterValue;
                        commitValue();
                    }
                    else if (state == ParseState.QuotedValue)
                        buffer.Append(current);

                    continue;
                }
                if (state == ParseState.Name)
                {
                    if (IsNameCharacter(current))
                    {
                        buffer.Append(current);
                        continue;
                    }
                }
                appendCharacter(current);
            }
            if (buffer.Length > 0 &&
                (state == ParseState.Value ||
                 state == ParseState.None ||
                 state == ParseState.AfterValue))
            {
                commitValue();
            }
            else if (buffer.Length > 0 && state == ParseState.Name)
                commitName();
            commitInput();
            return inputs;
        }
        
        char ConvertToSpecialCharacter(char c)
        {
            switch(c)
            {
                case '0': return '\0';
                case 'a': return '\a';
                case 'b': return '\b';
                case 'f': return '\f';
                case 'r': return '\r';
                case 'n': return '\n';
                case 't': return '\t';
                case 'v': return '\v';
            }
            return c;
        }

        bool IsNameCharacter(char current)
        {
            return (current >= 'a' && current <= 'z') ||
                   (current >= 'A' && current <= 'Z') ||
                   (current >= '0' && current <= '9') ||
                   current == '_';
        }

        enum ParseState
        {
            None,
            Name,
            BeforeValue,
            Value,
            QuotedValue,
            Array,
            AfterValue
        }
    }
}
