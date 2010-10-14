using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.Repositories;
using OpenWrap.Services;

namespace OpenWrap.Commands.Wrap
{
    [Command(Noun="wrap", Verb="list")]
    public class ListWrapCommand : AbstractCommand
    {
        [CommandInput(Position=0)]
        public string Query { get; set; }

        [CommandInput]
        public bool System { get; set; }

        [CommandInput]
        public string Remote { get; set; }

        protected IEnvironment Environment { get { return Services.Services.GetService<IEnvironment>(); } }
        public override IEnumerable<ICommandOutput> Execute()
        {
            var repoToList = GetRepositoryToList();
            if (repoToList == null)
                return new[] { new GenericError("Selected repository wasn't found. If you used -remote, make sure the remote repository exists.") };

            var packageList = repoToList.PackagesByName.NotNull();
            if (!string.IsNullOrEmpty(Query))
            {
                var filter = Query.Wildcard();
                packageList = packageList.Where(x => filter.IsMatch(x.Key));
            }
            return packageList
                    .Select(x => (ICommandOutput)new PackageDescriptionOutput(x.Key, x));
        }

        IPackageRepository GetRepositoryToList()
        {
            if (System)
                return Environment.SystemRepository;
            if (!string.IsNullOrEmpty(Remote))
                return Environment.RemoteRepositories.FirstOrDefault(x => x.Name.Equals(Remote, StringComparison.OrdinalIgnoreCase));
            return Environment.ProjectRepository;
        }
    }

    public class PackageDescriptionOutput : GenericMessage
    {
        public string PackageName { get; set; }

        public PackageDescriptionOutput(string packageName, IEnumerable<IPackageInfo> packageVersions)
            : base(" - {0}\r\n   Versions: {1}", packageName, string.Join(", ", packageVersions.Select(x => x.Version.ToString()).ToArray()))
        {
            PackageName = packageName;
        }
    }
}
