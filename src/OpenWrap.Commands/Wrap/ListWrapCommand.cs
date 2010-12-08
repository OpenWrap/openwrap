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

        string _remote;
        bool _remoteSet;

        [CommandInput(IsValueRequired = false)]
        public string Remote
        {
            get { return _remote; }
            set 
            {
                _remote = value;
                _remoteSet = true;
            }
        }

        protected IEnvironment Environment { get { return Services.Services.GetService<IEnvironment>(); } }
        public override IEnumerable<ICommandOutput> Execute()
        {
            var repoToList = GetRepositoryToList();
            if (repoToList == null)
                return new[] { new Error("Selected repository wasn't found. If you used -remote, make sure the remote repository exists.") };

            var packageList = repoToList.SelectMany(x=>x.PackagesByName.NotNull());
            if (!string.IsNullOrEmpty(Query))
            {
                var filter = Query.Wildcard();
                packageList = packageList.Where(x => filter.IsMatch(x.Key));
            }
            return packageList
                    .Select(x => (ICommandOutput)new PackageDescriptionOutput(x.Key, x));
        }

        IEnumerable<IPackageRepository> GetRepositoryToList()
        {
            if (System)
                return new[]{ Environment.SystemRepository };
           else if (_remoteSet && string.IsNullOrEmpty(Remote))
                return Environment.RemoteRepositories;
            else if (_remoteSet)
                return new[]{Environment.RemoteRepositories.FirstOrDefault(x => x.Name.EqualsNoCase(Remote))};
            return new[]{Environment.ProjectRepository};
        }
    }
}
