using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace OpenWrap.Configuration
{
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
}