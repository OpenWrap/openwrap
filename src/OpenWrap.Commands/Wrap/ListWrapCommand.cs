using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using OpenRasta.Client;
using OpenWrap.Collections;
using OpenWrap.Configuration;
using OpenWrap.PackageManagement;
using OpenWrap.Repositories;

namespace OpenWrap.Commands.Wrap
{
    [Command(Noun = "wrap", Verb = "list", IsDefault = true)]
    public class ListWrapCommand : WrapCommand
    {
        string _remote;
        bool _remoteSet;

        [CommandInput]
        public bool IgnoreInvalidSSLCert { get; set; }

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
        public bool System
        {
            get { return _system ?? (!_remoteSet && HostEnvironment.ProjectRepository == null); }
            set { _system = true; }
        }

        bool? _project;
        bool? _system;

        [CommandInput]
        public bool IncludeDependencies { get; set; }

        [CommandInput]
        public bool Project
        {
            get { return _project ?? (_system == null && !_remoteSet && HostEnvironment.ProjectRepository != null); }
            set { _project = value; }
        }

        public IEnumerable<IPackageRepository> SelectedRemotes { get; set; }

        public ListWrapCommand()
        {
            SelectedRemotes = Enumerable.Empty<IPackageRepository>();
        }
        protected override IEnumerable<ICommandOutput> ExecuteCore()
        {
            var repoToList = GetRepositoryToList();
            Func<string, bool> packageNameSelector = null;
            if (Query != null)
                packageNameSelector = _ => Query.Wildcard().IsMatch(_);

            if (_remoteSet && repoToList.Empty())
            {
                yield return new Error("Remote repository was not found.");
                foreach (var m in HintRemoteRepositories())
                    yield return m;
                yield break;
            }
            if (IgnoreInvalidSSLCert)
            {
                ServicePointManager.ServerCertificateValidationCallback
                    += new RemoteCertificateValidationCallback(SslCertificateValidators.ValidateAnyRemoteCertificate);
            }
            if (Project)
            {
                foreach(var descriptor in HostEnvironment.ScopedDescriptors
                    .OrderBy(x=>x.Key.Length))
                {

                    var resolvedPackages = PackageManager.ListProjectPackages(
                        descriptor.Value.Value,
                        HostEnvironment.ProjectRepository).ToList();

                    var lockedRepository = HostEnvironment.ProjectRepository.Feature<ISupportLocking>();
                    var packages = new DescriptorPackages(
                        descriptor.Key,
                        descriptor.Value.Value.Dependencies,
                        resolvedPackages,
                        IncludeDependencies,
                        packageNameSelector,
                        lockedRepository == null ? null : lockedRepository.LockedPackages[string.Empty]
                        );
                    yield return !packages.Packages.Any() ? new NoPackages(new[]{HostEnvironment.ProjectRepository}, search: Query) : (ICommandOutput)packages;

                }
            }
            var emptyRepos = new List<IPackageRepository>();

            foreach (var repository in repoToList)
            {
                var packages = PackageManager.ListPackages(new[]{repository}, Query).OfType<PackageFoundResult>();
                if (packages.Any())
                 yield return new RepositoryPackages(repository, packages);
                else
                    emptyRepos.Add(repository);
            }
            if (emptyRepos.Any())
            {
                yield return new NoPackages(emptyRepos, Query);
            }
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
                foreach(var fetch in Remotes.FetchRepositories(Remote))
                    yield return fetch;
                yield break;
            }
        }
        protected override IEnumerable<Func<IEnumerable<ICommandOutput>>> Validators()
        {
            yield return SetRemotes;
            yield return VerifyProject;
        }

        IEnumerable<ICommandOutput> VerifyProject()
        {
            if (_project == true && HostEnvironment.ProjectRepository == null)
                yield return new NotInProject();
        }

        IEnumerable<ICommandOutput> SetRemotes()
        {
            if (_remoteSet)
                SelectedRemotes = string.IsNullOrEmpty(Remote)
                    ? Remotes.FetchRepositories()
                    : new[]{Remotes.FetchRepositories(Remote).First()};
            yield break;
        }
    }

    public class Node<T> : Node
    {
        readonly T _value;
        readonly Action<TreeRenderer, T> _renderer;

        public Node(T value, Action<TreeRenderer, T> renderer)
        {
            _value = value;
            _renderer = renderer;
        }

        public override void Render(TreeRenderer tree)
        {
            _renderer(tree, _value);
        }
    }
}