using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace OpenWrap.PackageModel.Parsers
{
    public abstract class AbstractDescriptorParser 
    {
        readonly Regex _regex;

        protected AbstractDescriptorParser(string header)
        {
            Header = header;
            _regex = new Regex(@"^\s*" + header + @"\s*:\s*(?<content>.*)$", RegexOptions.IgnoreCase);
        }

        protected string Header { get; set; }

        public void Parse(string line, IPackageDescriptor descriptor)
        {
            var match = _regex.Match(line);
            if (!match.Success)
                return;
            ParseContent(match.Groups["content"].Value, descriptor);
        }

        public IEnumerable<string> Write(PackageDescriptor descriptor)
        {
            var content = WriteContent(descriptor).ToList();
            if (content.Count == 0) return content;
            return content.Select(x => Header + ": " + x);
        }

        protected abstract void ParseContent(string content, IPackageDescriptor descriptor);
        protected abstract IEnumerable<string> WriteContent(PackageDescriptor descriptor);
    }
}