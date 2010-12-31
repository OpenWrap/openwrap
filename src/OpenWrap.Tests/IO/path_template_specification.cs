using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using OpenWrap.Testing;

namespace OpenWrap.Tests.IO
{
    public class path_template_specification : context
    {
        [Test]
        public void var_name_is_correct()
        {
            PathTemplate.TryParse("{source}").Name.ShouldBe("source");
        }
        [Test]
        public void simple_parameter_is_parsed()
        {
            PathTemplate.TryParse("{source}").Value("src").ShouldBe("src");
        }
        [Test]
        public void parameter_with_value_is_parsed()
        {
            PathTemplate.TryParse("{source: src}").Value("src").ShouldBe("src");
            PathTemplate.TryParse("{source: src}").Value("source").ShouldBe(null);
        }
        [Test]
        public void parameter_with_value_is_replaced_with_transform()
        {
            PathTemplate.TryParse("{source: src=source}").Value("src").ShouldBe("source");
            PathTemplate.TryParse("{source: src=source}").Value("source").ShouldBe(null);
        }
    }

    public class PathTemplate
    {
        public string Name { get; private set; }
        readonly string _val;
        readonly string _transformed;
        readonly string _source;
        static Regex _regex = new Regex(@"^\s* {\s* (?<var>[a-zA-Z][a-zA-Z0-9]*) \s* (:?\s* [""']? (?<val>.+?) [""']?  ( \s* = \s*  (?<valTransformed>.+?))? )? \s*} \s*$", RegexOptions.IgnorePatternWhitespace);

        public static PathTemplate TryParse(string templateString)
        {
            var match = _regex.Match(templateString);
            if (!match.Success) return null;

            var var = match.Groups["var"].Value;
            var val = match.Groups["val"].Success ? match.Groups["val"].Value : null;
            var transformed = match.Groups["valTransformed"].Success ? match.Groups["valTransformed"].Value : null;
            return new PathTemplate(var, val, transformed);
        }

        PathTemplate(string var, string val, string transformed)
        {
            Name = var;
            _val = val;
            _transformed = transformed;
        }

        public string Value(string input)
        {
            if (_val == null)
                return input;
            if (input.EqualsNoCase(_val))
                return _transformed ?? _val;
            return null;
        }
    }
}
