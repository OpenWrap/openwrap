using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.Collections;
using OpenWrap.Commands.Remote;
using OpenWrap.Configuration;
using OpenWrap.PackageManagement;
using OpenWrap.PackageModel;
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

        [CommandInput(IsValueRequired = false)]
        public bool Detailed { get; set; }

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

            var currentPackages = CurrentPackages();

            foreach (var m in PackageManager.ListPackages(repoToList, Query, Detailed ? PackageListOptions.Detailed : PackageListOptions.Default, currentPackages))
                yield return ToOutput(m);
        }

        IEnumerable<IPackageInfo> CurrentPackages()
        {
            return HostEnvironment.ProjectRepository != null ? HostEnvironment.ProjectRepository.PackagesByName.NotNull().SelectMany(x => x) : Enumerable.Empty<IPackageInfo>();
        }

        IEnumerable<IPackageRepository> GetRepositoryToList()
        {
            if (System)
            {
                if (HostEnvironment.SystemRepository != null)
                    yield return HostEnvironment.SystemRepository;
                yield break;
            }
            if (_remoteSet && string.IsNullOrEmpty(Remote))
            {

                foreach (var remote in HostEnvironment.RemoteRepositories.NotNull())
                    yield return remote;
            }
            if (_remoteSet)
            {
                var repo = GetRemoteRepository(Remote);
                if (repo != null)
                    yield return repo;
                yield break;
            }
            if (HostEnvironment.ProjectRepository != null)
                yield return HostEnvironment.ProjectRepository;
        }
    }
}
