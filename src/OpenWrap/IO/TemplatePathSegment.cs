using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OpenWrap.Collections;

namespace OpenWrap.IO
{
    public class TemplatePathSegment : PathSegment
    {
        public string Name { get; private set; }
        readonly string _val;
        readonly string _transformed;
        static Regex _regex = new Regex(@"{\s* (?<var>[a-zA-Z][a-zA-Z0-9]*?) \s* (:\s* [""']? (?<val>.+?) [""']?  ( \s* = \s*  (?<valTransformed>.+?))? )? \s*}", RegexOptions.IgnorePatternWhitespace);

        public static PathSegment TryParse(string templateString)
        {
            
            var matches = _regex.Matches(templateString);
            var templateGroups = (from match in matches.OfType<Match>()
                                  where match.Success
                                  let var = match.Groups["var"].Value
                                  let val = match.Groups["val"].Success ? match.Groups["val"].Value : null
                                  let transformed = match.Groups["valTransformed"].Success ? match.Groups["valTransformed"].Value : null
                                  select new{ 
                                      VarName = var,
                                      Value = val,
                                      Transformed = transformed,
                                      Left = match.Index,
                                      Right = match.Index + match.Length
                                  }).ToList();
            if (templateGroups.Count == 0) return null;
            if (templateGroups.Count == 1 &&
                templateGroups[0].Left == 0 &&
                templateGroups[0].Right == templateString.Length)
                return new TemplatePathSegment(templateGroups[0].VarName, templateGroups[0].Value, templateGroups[0].Transformed);

            int lastPosition = 0;
            string parserRegex = string.Empty;
            var transformedValues = new Dictionary<string, string>();

            for (int i = 0; i < templateGroups.Count; i++)
            {
                
                var template = templateGroups[i];
                if (template.Transformed != null)
                    transformedValues[template.VarName] = template.Transformed;
                if (template.Left - lastPosition > 0)
                    parserRegex += Regex.Escape(templateString.Substring(lastPosition, template.Left - lastPosition)).Replace(@"\*", ".*").Replace(@"\?", @"\.?");

                parserRegex += string.Format("(?<{0}>{1})", template.VarName, template.Value != null ? Regex.Escape(template.Value) : ".*?");
                lastPosition = template.Right;
                string leftover = templateString.Length - template.Right > 0
                    ? templateString.Substring(template.Right) 
                    : null;
                if (i == templateGroups.Count-1 && leftover != null)
                {
                    parserRegex += Regex.Escape(leftover).Replace(@"\*", ".*").Replace(@"\?", @"\.?");
                }
            }
            return new CompoundTemplatePathSegment("^" + parserRegex + "$", templateGroups.Select(x=>x.VarName), transformedValues);
        }

        TemplatePathSegment(string var, string val, string transformed)
        {
            Name = var;
            _val = val;
            _transformed = transformed;
        }

        public override bool TryParse(IDictionary<string, string> properties, LinkedListNode<PathSegment> currentParser, ref LinkedListNode<string> currentSegment)
        {
            if (_val == null)
                properties[Name] = currentSegment.Value;
            else if (_val.EqualsNoCase(currentSegment.Value))
                properties[Name] = _transformed ?? currentSegment.Value;
            else return false;

            currentSegment = currentSegment.Next;
            return true;
        }
    }
    public class CompoundTemplatePathSegment : PathSegment
    {
        readonly ICollection<string> _varNames;
        readonly IDictionary<string, string> _transformedValues;
        Regex _regex;

        public CompoundTemplatePathSegment(string parserRegex, IEnumerable<string> varNames, IDictionary<string, string> transformedValues)
        {
            _varNames = varNames.ToList();
            _transformedValues = transformedValues;
            _regex = new Regex(parserRegex, RegexOptions.IgnoreCase);
        }

        public override bool TryParse(IDictionary<string, string> properties, LinkedListNode<PathSegment> currentParser, ref LinkedListNode<string> currentSegment)
        {
            var match = _regex.Match(currentSegment.Value);
            if (match.Success == false) return false;

            foreach (var val in from var in _varNames
                                let parsedValue = match.Groups[var].Value
                                select new { var, value = _transformedValues.ContainsKey(var) ? _transformedValues[var] : parsedValue })
                properties[val.var] = val.value;

            currentSegment = currentSegment.Next;
            return true;
        }
    }
}