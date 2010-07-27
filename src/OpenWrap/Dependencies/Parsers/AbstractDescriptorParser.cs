using System.Text.RegularExpressions;

namespace OpenWrap.Dependencies
{
    public abstract class AbstractDescriptorParser : IDescriptorParser
    {
        protected string Header { get; set; }
        Regex _regex;

        public AbstractDescriptorParser(string header)
        {
            Header = header;
            _regex = new Regex(@"^\s*" + header + @"\s*:\s*(?<content>.*)$", RegexOptions.IgnoreCase);
        }

        public void Parse(string line, WrapDescriptor descriptor)
        {
            var match = _regex.Match(line);
            if (!match.Success)
                return;
            ParseContent(match.Groups["content"].Value, descriptor);
        }
        public virtual void ParseContent(string content, WrapDescriptor descriptor)
        {
            
        }
        public virtual string GetContentRegex()
        {
            return @".*";
        }
   }
}