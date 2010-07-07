using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using OpenWrap.IO;

namespace OpenWrap.Configuration
{
    public interface IConfigurationManager
    {
        T Load<T>(Uri uri);
        void Save<T>(Uri uri, T configEntry);
    }

    public class ConfigurationManager : IConfigurationManager
    {
        readonly IDirectory _configurationDirectory;
        public Uri BaseUri { get; private set; }
        public ConfigurationManager(IDirectory configurationDirectory)
        {
            _configurationDirectory = configurationDirectory;
            BaseUri = ConfigurationEntries.BaseUri;
        }

        public T Load<T>(Uri uri)
        {
            var relativeUri = ConfigurationEntries.BaseUri.MakeRelativeUri(uri).ToString();
            var file = _configurationDirectory.GetFile(relativeUri);
            if (file == null)
                return default(T);
            return ParseFile<T>(file);
        }

        T ParseFile<T>(IFile file)
        {
            string data;
            using (var fileStream = file.OpenRead())
                data = Encoding.UTF8.GetString(fileStream.ReadToEnd());

            if (typeof(T).GetInterfaces().Any(x=>x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDictionary<,>) && x.GetGenericArguments()[0] == typeof(string)))
            {
                // index all sections by string
            }
            return default(T);
        }

        public void Save<T>(Uri uri, T configEntry)
        {
            throw new NotImplementedException();
        }
    }
    public class ConfigurationParser
    {

        static Regex _configurationSectionRegex = new Regex(@"^\s*\[(?<type>\w+?)(\s+(?<name>\w+)\s*)?]\s*$");
        static Regex _configurationLineRegex = new Regex(@"^\s*(?<name>\w+)\s*=\s*(?<value>.*?)\s*$");

        public IEnumerable<ConfigurationEntry> Parse(string data)
        {
            ConfigurationSection currentSection = null;
            foreach (var line in data.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                var sectionMatch = _configurationSectionRegex.Match(line);
                if (sectionMatch.Success)
                {
                    if (currentSection != null)
                        yield return currentSection;
                    currentSection = new ConfigurationSection
                    {
                        Type = sectionMatch.Groups["type"].Value,
                        Name = sectionMatch.Groups["name"].Success ? sectionMatch.Groups["name"].Value : string.Empty
                    };
                    continue;
                }
                var lineMatch = _configurationLineRegex.Match(line);
                if (lineMatch.Success)
                {
                    var configLine = new ConfigurationLine
                    {
                        Name = lineMatch.Groups["name"].Value,
                        Value = lineMatch.Groups["value"].Value
                    };
                    if (currentSection != null)
                        currentSection.Lines.Add(configLine);
                    else
                        yield return configLine;
                }
            }
            if (currentSection != null)
                yield return currentSection;
        }
    }
    public class ConfigurationEntry
    {
    }

    public class ConfigurationSection : ConfigurationEntry
    {
        public ConfigurationSection()
        {
            Lines = new List<ConfigurationLine>();
        }
        public string Type { get; set; }
        public string Name { get; set; }
        public ICollection<ConfigurationLine> Lines { get; private set; }
    }
    public class ConfigurationLine : ConfigurationEntry
    {
        public string Name { get; set;}
        public string Value { get; set;}
    }
    public static class ConfigurationEntries
    {
        public static readonly Uri RemoteRepositories = new Uri("http://configuration.openwrap.org/remote-repositories");

        public static readonly Uri BaseUri = new Uri("http://configuration.openwrap.org");

    }
}
