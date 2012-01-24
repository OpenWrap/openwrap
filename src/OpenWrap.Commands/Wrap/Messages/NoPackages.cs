using System.Collections.Generic;
using System.Linq;
using OpenWrap.Repositories;

namespace OpenWrap.Commands.Wrap
{
    public class NoPackages : Info
    {
        public IEnumerable<IPackageRepository> Repositories { get; set; }
        public string Search { get; set; }

        public NoPackages(IEnumerable<IPackageRepository> repositories, string search = null)
        {
            Repositories = repositories;
            Search = search;
        }
        public override string ToString()
        {
            var template = Search != null
                               ? "No package found for '{0}' in the following repositories: {1}"
                               : "No packages in the following repositories: {1}";
            return string.Format(template, Search, Repositories.Select(_ => _.Name).JoinString(", "));
        }
    }
}