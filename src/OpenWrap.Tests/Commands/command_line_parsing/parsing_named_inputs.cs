using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Commands.command_line_parsing
{
    public class InputParser
    {
        const char ESCAPE = '`';
        const char DBL_QUOTE = '"';
        const char SINGLE_QUOTE = '\'';
        public IEnumerable<Input> Parse(string value)
        {
            bool isEscape;
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

    public class InputParserException : Exception
    {
        public InputParserException(string message) : base(message)
        {
        }
    }
    [TestFixture("rohan, mordor, 'old forest road'", "rohan", "mordor", "old forest road")]
    [TestFixture("rohan, \"mordor\", 'old forest road'", "rohan", "mordor", "old forest road")]
    [TestFixture("rohan, \"mordor\", 'old forest road'", "rohan", "mordor", "old forest road")]
    [TestFixture("rohan`,, \"mordor,\", 'old forest road, The'", "rohan,", "mordor,", "old forest road, The")]
    class multiple_values : contexts.input_parser
    {
        readonly string _expectedFirst;
        readonly string _expectedSecond;
        readonly string _expectedThird;

        public multiple_values(string line, string expectedFirst, string expectedSecond, string expectedThird)
        {
            _expectedFirst = expectedFirst;
            _expectedSecond = expectedSecond;
            _expectedThird = expectedThird;
            when_parsing(line);
        }

        [Test]
        public void should_have_multiple_values()
        {
            Result.Single().ShouldBeOfType<MultiValueInput>().Values.ShouldHaveCountOf(3);
        }

        [Test]
        public void values_are_parsed()
        {
            var values = Result.Single().ShouldBeOfType<MultiValueInput>().Values;
            values.ElementAt(0).ShouldBe(_expectedFirst);
            values.ElementAt(1).ShouldBe(_expectedSecond);
            values.ElementAt(2).ShouldBe(_expectedThird);
        }
    }
    [TestFixture("`0", "\0")]
    [TestFixture("`a", "\a")]
    [TestFixture("`b", "\b")]
    [TestFixture("`f", "\f")]
    [TestFixture("`n", "\n")]
    [TestFixture("`r", "\r")]
    [TestFixture("`t", "\t")]
    [TestFixture("`v", "\v")]
    class escape_special_characters : contexts.input_parser
    {
        readonly string _expected;

        public escape_special_characters(string line, string expected)
        {
            _expected = expected;
            when_parsing(line);
        }

        [Test]
        public void special_character_appended()
        {
            Result.Single().ShouldBeOfType<SingleValueInput>().Value.ShouldBe(_expected);
        }
    }
    [TestFixture(@"`-named", "", "-named")]
    [TestFixture(@"-named `""value", "named", "\"value")]
    [TestFixture("-named \"tom `\"bombadil`\"\"", "named", "tom \"bombadil\"")]
    class escape_characters : contexts.input_parser
    {
        readonly string _expectedName;
        readonly string _expectedValue;

        public escape_characters(string line, string expectedName, string expectedValue)
        {
            _expectedName = expectedName;
            _expectedValue = expectedValue;
            when_parsing(line);
        }

        [Test]
        public void name_is_parsed()
        {
            Result.Single().Name.ShouldBe(_expectedName);
        }
        [Test]
        public void value_is_parsed()
        {
            Result.Single().ShouldBeOfType<SingleValueInput>().Value.ShouldBe(_expectedValue);
        }
    }
    [TestFixture(@"-named "" tom bombadil""", " tom bombadil")]
    [TestFixture(@"-named ' tom bombadil'", " tom bombadil")]
    [TestFixture(@"-named ' tom ""bombadil""'", " tom \"bombadil\"")]
    [TestFixture(@"-named "" tom o'bombadil""", " tom o'bombadil")]
    class named_value_with_quotes : contexts.input_parser
    {
        readonly string _expected;

        public named_value_with_quotes(string line, string expected)
        {
            _expected = expected;
            when_parsing(line);
        }

        [Test]
        public void has_name()
        {
            Result.First().Name.ShouldBe("named");
        }

        [Test]
        public void has_value_preserving_white_space()
        {
            Result.First().ShouldBeOfType<SingleValueInput>()
                    .Value.ShouldBe(_expected);
        }
    }
    [TestFixture("somewhere -named one-ring -evil")]
    [TestFixture("-evil -named one-ring somewhere")]
    [TestFixture("-named one-ring somewhere -evil")]
    class mixed_named_and_unnamed_values : contexts.input_parser
    {
        public mixed_named_and_unnamed_values(string line)
        {
            when_parsing(line);
        }

        [Test]
        public void named_with_value_parsed()
        {
            Result.FirstOrDefault(x => x.Name == "named")
                    .ShouldNotBeNull()
                    .ShouldBeOfType<SingleValueInput>()
                    .Value.ShouldBe("one-ring");
        }
        [Test]
        public void named_without_value_parsed()
        {
            Result.FirstOrDefault(x => x.Name == "evil")
                    .ShouldNotBeNull()
                    .ShouldBeOfType<SingleValueInput>()
                    .Value.ShouldBe(string.Empty);
        }
        [Test]
        public void value_without_name_parsed()
        {
            Result.FirstOrDefault(x => x.Name == string.Empty)
                    .ShouldNotBeNull()
                    .ShouldBeOfType<SingleValueInput>()
                    .Value.ShouldBe("somewhere");
        }
    }
    class multiple_named_inputs_with_single_values : contexts.input_parser
    {
        public multiple_named_inputs_with_single_values()
        {
            when_parsing("-named one-ring -location mordor");
        }

        [Test]
        public void two_inputs_parsed()
        {
            Result.ShouldHaveCountOf(2);
        }

        [Test]
        public void names_are_parsed()
        {
            Result.ElementAt(0).Name.ShouldBe("named");
            Result.ElementAt(1).Name.ShouldBe("location");
        }

        [Test]
        public void values_are_parsed()
        {
            Result.ElementAt(0).ShouldBeOfType<SingleValueInput>().Value.ShouldBe("one-ring");
            Result.ElementAt(1).ShouldBeOfType<SingleValueInput>().Value.ShouldBe("mordor");

        }

    }
    class singe_named_input_no_value : contexts.input_parser
    {
        public singe_named_input_no_value()
        {
            when_parsing("-named");
        }

        [Test]
        public void name_is_parsed()
        {
            Result.First().Name.ShouldBe("named");
        }

        [Test]
        public void value_is_empty()
        {
            Result.First().ShouldBeOfType<SingleValueInput>().Value.ShouldBe(string.Empty);
        }
    }
    class single_noname_input : contexts.input_parser
    {
        public single_noname_input()
        {
            when_parsing("one-ring");
        }

        [Test]
        public void one_input_parsed()
        {
            Result.ShouldHaveCountOf(1);
        }

        [Test]
        public void name_is_empty()
        {
            Result.First().Name.ShouldBe(string.Empty);
        }

        [Test]
        public void value_is_parsed()
        {
            Result.First().ShouldBeOfType<SingleValueInput>().Value.ShouldBe("one-ring");
        }
    }
    class single_named_input : contexts.input_parser
    {
        public single_named_input()
        {
            when_parsing("-named one-ring");
        }
        [Test]
        public void one_input_parsed()
        {
            Result.ShouldHaveCountOf(1);
        }
        [Test]
        public void name_is_parsed()
        {
            Result.First().Name.ShouldBe("named");
        }
        [Test]
        public void value_is_parsed()
        {
            Result.First().ShouldBeOfType<SingleValueInput>().Value.ShouldBe("one-ring");
        }
    }

    class empty_input : contexts.input_parser
    {
        public empty_input()
        {
            when_parsing("");
        }

        [Test]
        public void no_input_is_parsed()
        {
            Result.ShouldBeEmpty();
        }
    }

    public abstract class Input
    {
        public string Name { get; set; }

    }

    public class SingleValueInput : Input
    {
        public string Value { get; set; }
    }

    public class MultiValueInput : Input
    {
        public ICollection<string> Values { get; set; }
    }

    namespace contexts
    {
        public abstract class input_parser : context
        {
            public void when_parsing(string input)
            {
                Result = new InputParser().Parse(input);
            }

            protected IEnumerable<Input> Result { get; set; }
        }
    }
}
