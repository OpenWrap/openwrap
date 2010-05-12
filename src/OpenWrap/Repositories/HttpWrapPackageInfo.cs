using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Wrap.Dependencies;
using OpenRasta.Wrap.Sources;

namespace OpenWrap.Repositories
{
    public class HttpWrapPackageInfo : IWrapPackageInfo
    {
        readonly IHttpNavigator _httpNavigator;
        string _link;


        public HttpWrapPackageInfo(IHttpNavigator httpNavigator, string name, string version, string link, IEnumerable<string> depends)
        {
            Name = name;
            Version = new Version(version);
            _httpNavigator = httpNavigator;
            _link = link;
            Dependencies = (from dependency in depends
                            let strings = dependency.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)
                            where strings.Length >= 1
                            let dependencyName = strings[0]
                            where !string.IsNullOrEmpty(dependencyName)
                            select new WrapDependency
                            {
                                Name = dependencyName,
                                VersionVertices = WrapDependencyParser.Parse(strings.Skip(1).ToArray()).ToList()
                            }).ToList();
        }

        public ICollection<WrapDependency> Dependencies { get; set; }
        public string Name { get; set; }
        public Version Version { get; set; }

        public IWrapPackage Load()
        {
            return null;
        }
    }
}