using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using OpenFileSystem.IO;
using OpenWrap.IO;

namespace OpenWrap.PackageModel.Serialization
{
    public class PackageDescriptorReader
    {
        static readonly Regex _lineParser = new Regex(@"^\s*(?<fieldname>[0-9a-zA-Z\-\.]*?)\s*:\s*(?<fieldvalue>.*?)\s*$", RegexOptions.Compiled);


        public IPackageDescriptor Read(Stream content)
        {
            return Read(content, entries => new PackageDescriptor(entries));
        }

        public IPackageDescriptor Read(IFile filePath)
        {
            if (!filePath.Exists)
                throw new FileNotFoundException("The file does not exist.", filePath.Path);
            return filePath.ReadRetry(Read);
        }

        public T Read<T>(Stream content, Func<IEnumerable<IPackageDescriptorEntry>, T> ctor)
            where T : class, IPackageDescriptor
        {
            var stringReader = new StreamReader(content, true);
            var lines = stringReader.ReadToEnd().GetUnfoldedLines();
            if (lines.Any(l => l.Contains('\n')))
                throw new InvalidPackageException(
                    "Package descriptor contains invalid line endings (<LF> instead of <CR><LF>). Either the descriptor is in the wrong format or a source control system that plays around with line endings (like git) has been misconfigured (core.autocrlf may help).");

            return ctor(lines.Select(ParseLine));
        }

        public IDictionary<string, FileBased<IPackageDescriptor>> ReadAll(IDirectory directoryToReadFrom)
        {
            var descriptorFiles = (from dir in directoryToReadFrom.AncestorsAndSelf()
                                   let descriptors = dir.Files("*.wrapdesc")
                                   where descriptors.Count() > 0
                                   select descriptors.OrderBy(x => x.Name.Length)).FirstOrDefault();
            var all = new Dictionary<string, FileBased<IPackageDescriptor>>();
            if (descriptorFiles == null)
                return all;

            var root = descriptorFiles.First();
            var rootDescriptor = root.ReadRetry(Read);
            all[string.Empty] = FileBased.New(root, rootDescriptor);
            foreach (var descriptor in descriptorFiles.Skip(1))
            {
                var regex = new Regex(string.Format("^{0}.(?<scope>.*).wrapdesc$", rootDescriptor.Name));
                var scopeMatch = regex.Match(descriptor.Name);
                if (scopeMatch.Success == false)
                    continue;
                var scopedDescriptor = rootDescriptor
                    .CreateScoped(descriptor.Read(Read));
                all[scopeMatch.Groups["scope"].Value] = FileBased.New(descriptor, scopedDescriptor);
            }
            return all;
        }

        static IPackageDescriptorEntry ParseLine(string line)
        {
            var trimmedLine = line.TrimStart();
            if (trimmedLine.StartsWith("#"))
                return new GenericDescriptorEntry("Comment", trimmedLine.Substring(1).TrimStart());

            var match = _lineParser.Match(line);
            return !match.Success
                       ? null
                       : new GenericDescriptorEntry(
                             match.Groups["fieldname"].Value,
                             match.Groups["fieldvalue"].Value);
        }
    }
}