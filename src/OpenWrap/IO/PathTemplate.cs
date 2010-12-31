using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace OpenWrap.IO
{
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
    public class PathTemplateResolver
    {
        public PathTemplateResolver(IEnumerable<PathTemplate> templates)
        {
            
        }
    }
}