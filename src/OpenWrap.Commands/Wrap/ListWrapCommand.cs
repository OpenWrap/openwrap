using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Collections;
using OpenWrap.Configuration;
using OpenWrap.Repositories;

namespace OpenWrap.Commands.Wrap
{
    [Command(Noun = "wrap", Verb = "list", IsDefault = true)]
    public class ListWrapCommand : WrapCommand
    {
        string _remote;
        bool _remoteSet;

        [CommandInput(Position = 0)]
        public string Query { get; set; }

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

        [CommandInput]
        public bool System { get; set; }

        protected override IEnumerable<ICommandOutput> ExecuteCore()
        {
            var repoToList = GetRepositoryToList();
            if (_remoteSet && repoToList.Empty())
            {
                yield return new Error("Remote repository was not found.");
                foreach (var m in HintRemoteRepositories())
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
                if (HostEnvironment.SystemRepository != null)
                    yield return HostEnvironment.SystemRepository;
                yield break;
            }
            if (_remoteSet)
            {

                foreach (var remote in GetFetchRepositories(Remote))
                    yield return remote;

            }
            if (HostEnvironment.ProjectRepository != null)
                yield return HostEnvironment.ProjectRepository;
        }
    }
}