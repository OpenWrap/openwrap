using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.Repositories;
using OpenWrap.Services;

namespace OpenWrap.Commands.Wrap
{
    [Command(Noun = "wrap", Verb = "list")]
    public class ListWrapCommand : WrapCommand
    {
        [CommandInput(Position = 0)]
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
                yield return new Error("Selected repository wasn't found. If you used -remote, make sure the remote repository exists.");

            foreach (var m in PackageManager.ListPackages(repoToList, Query))
                yield return ToOutput(m);
        }

        IEnumerable<IPackageRepository> GetRepositoryToList()
        {
            if (System)
                return new[] { Environment.SystemRepository };
            else if (_remoteSet && string.IsNullOrEmpty(Remote))
                return Environment.RemoteRepositories;
            else if (_remoteSet)
                return new[] { Environment.RemoteRepositories.FirstOrDefault(x => x.Name.EqualsNoCase(Remote)) };
            return new[] { Environment.ProjectRepository };
        }
    }
}
