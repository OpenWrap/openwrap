using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.Collections;
using OpenWrap.Commands.Remote;
using OpenWrap.Configuration;
using OpenWrap.Repositories;
using OpenWrap.Runtime;
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
            if (_remoteSet && repoToList.Empty())
            {
                yield return new Error("Remote repository was not found.");
                foreach(var m in HintRemoteRepositories())
                    yield return m;
                yield break;
            }

            foreach (var m in PackageManager.ListPackages(repoToList, Query))
                yield return ToOutput(m);
        }

        IEnumerable<IPackageRepository> GetRepositoryToList()
        {
            if (System)
            {
                if (Environment.SystemRepository != null)
                    yield return Environment.SystemRepository;
                yield break;
            }
            if (_remoteSet && string.IsNullOrEmpty(Remote))
            {

                foreach (var remote in Environment.RemoteRepositories.NotNull())
                    yield return remote;
            }
            if (_remoteSet)
            {
                var repo = GetRemoteRepository(Remote);
                if (repo != null)
                    yield return repo;
                yield break;
            }
            if (Environment.ProjectRepository != null)
                yield return Environment.ProjectRepository;
        }
    }
}
