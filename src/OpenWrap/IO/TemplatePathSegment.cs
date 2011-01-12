using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OpenFileSystem.IO;
using OpenWrap.Collections;

namespace OpenWrap.IO
{
    public class TemplatePathSegment : PathSegment
    {
        public string Name { get; private set; }
        readonly string _val;
        readonly string _transformed;
        static Regex _regex = new Regex(@"^\s* {\s* (?<var>[a-zA-Z][a-zA-Z0-9]*) \s* (:?\s* [""']? (?<val>.+?) [""']?  ( \s* = \s*  (?<valTransformed>.+?))? )? \s*} \s*$", RegexOptions.IgnorePatternWhitespace);

        public static PathSegment TryParse(string templateString)
        {
            var match = _regex.Match(templateString);
            if (!match.Success) return null;

            var var = match.Groups["var"].Value;
            var val = match.Groups["val"].Success ? match.Groups["val"].Value : null;
            var transformed = match.Groups["valTransformed"].Success ? match.Groups["valTransformed"].Value : null;
            return new TemplatePathSegment(var, val, transformed);
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
    public class LiteralPathSegment : PathSegment
    {
        readonly string _path;

        public LiteralPathSegment(string path)
        {
            _path = path;
        }

        public override bool TryParse(IDictionary<string, string> properties, LinkedListNode<PathSegment> currentParser, ref LinkedListNode<string> currentSegment)
        {
            bool success = currentSegment != null && currentSegment.Value.EqualsNoCase(_path);
            if (success)
                currentSegment = currentSegment.Next;
            return success;
        }
    }
    public class PathSegmentResult
    {
        public PathSegmentResult(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; private set; }
        public string Value { get; private set; }
    }
    public class PathTemplateProcessor
    {
        LinkedList<PathSegment> _segments;
        string _searchString;

        public PathTemplateProcessor(string path)
        {
            var segments = new List<PathSegment>();
            var searchPath = new List<string>();
            var pathSegments = path.Split(new[] { '/', '\\' });

            if (pathSegments[0] != "**")
                segments.Add(new WildcardPathSegment());

            foreach (var seg in pathSegments)
            {
                var templateSegment = TemplatePathSegment.TryParse(seg);
                PathSegment segment;
                string searchSegment;
                if (seg == "**")
                {
                    segment = new WildcardPathSegment();
                    searchSegment = "**";
                }
                else if (templateSegment != null)
                {
                    segment = templateSegment;
                    searchSegment = "*";
                }
                else
                {
                    segment = new LiteralPathSegment(seg);
                    searchSegment = seg;
                }
                segments.Add(segment);
                searchPath.Add(searchSegment);
            }
            _segments = new LinkedList<PathSegment>(segments);
            _searchString = searchPath.Join(System.IO.Path.DirectorySeparatorChar);
        }
        
        public bool TryParsePath(Path path, out IDictionary<string,string> properties)
        {
            if (!path.IsRooted)
                throw new ArgumentException("'path' should be rooted.", "path");

            var segments = SkipToFirstSearchSegment(path);
            var parsers = _segments;
            properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (segments.Count == 0)
                return false;

            var currentSegment = segments.First;
            var currentParser = parsers.First;
            do
            {
                if (!currentParser.Value.TryParse(properties, currentParser, ref currentSegment))
                    return false;

                currentParser = currentParser.Next;
                if (currentParser == null && currentSegment != null)
                    return false;
            } while (currentParser != null);

            return true;
        }

        LinkedList<string> SkipToFirstSearchSegment(Path path)
        {
            return new LinkedList<string>(path.Segments);
        }

        public IEnumerable<PathTemplateItem<IDirectory>> Directories(IDirectory baseDirectory)
        {
            IDictionary<string, string> var = null;
            return from dir in baseDirectory.Directories(_searchString)
                   where TryParsePath(dir.Path, out var)
                   let dirVars = new Dictionary<string, string>(var, StringComparer.OrdinalIgnoreCase)
                   select new PathTemplateItem<IDirectory>(dir, dirVars);
        }
    }

    public class WildcardPathSegment : PathSegment
    {
        public override bool TryParse(IDictionary<string, string> properties, LinkedListNode<PathSegment> currentParser, ref LinkedListNode<string> currentSegment)
        {
            if (currentParser.Next == null)
                throw new ArgumentException("A wildcard segment cannot be at the end of a path.");

            var emptyProperties = new Dictionary<string, string>();
            var nextParser = currentParser.Next;
            var pathNode = currentSegment;

            while(pathNode != null)
            {
                var localPathNode = pathNode;
                var success = nextParser.Value.TryParse(emptyProperties, nextParser, ref localPathNode);
                if (!success)
                {
                    pathNode = pathNode.Next;
                }
                else
                {
                    currentSegment = pathNode;
                    return true;
                }
            }
            return false;
        }
    }

    public class PathTemplateItem<T> where T:IFileSystemItem
    {
        public PathTemplateItem(T item, IDictionary<string, string> parameters)
        {
            Item = item;
            Parameters = parameters;
        }

        public T Item { get; private set; }
        public IDictionary<string, string> Parameters { get; private set; }
    }

    public abstract class PathSegment
    {
        public abstract bool TryParse(IDictionary<string, string> properties, LinkedListNode<PathSegment> currentParser, ref LinkedListNode<string> currentSegment);
    }
}