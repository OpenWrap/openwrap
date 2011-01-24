using System;
using System.Collections.Generic;
using System.Linq;
using OpenFileSystem.IO;

namespace OpenWrap.IO
{
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
    public class PathFinder
    {
        public static IDirectory GetSourceDirectory(string[] templates, IDirectory dir)
        {
            IDictionary<string, string> parsedProperties;
            return (from t in templates
                    let processor = new PathTemplateProcessor(t)
                    from directory in processor.Directories(dir)
                    where directory.Parameters.ContainsKey("source") &&
                          directory.Parameters.Count == 1
                    select directory.Item).FirstOrDefault();
        }
    }
}