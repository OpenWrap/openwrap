using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Collections;
using OpenWrap.Configuration;
using OpenWrap.PackageManagement;
using OpenWrap.PackageModel;
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
        public bool System
        {
            get { return _system ?? (!_remoteSet && HostEnvironment.ProjectRepository == null); }
            set { _system = true; }
        }

        bool? _project;
        bool? _system;

        [CommandInput]
        public bool Detailed { get; set; }

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
            if (Project)
            {
                foreach(var descriptor in HostEnvironment.ScopedDescriptors
                    .OrderBy(x=>x.Key.Length))
                {

                    var resolvedPackages = PackageManager.ListProjectPackages(
                        descriptor.Value.Value,
                        HostEnvironment.ProjectRepository).ToList();

                    var lockedRepository = HostEnvironment.ProjectRepository.Feature<ISupportLocking>();
                    yield return new DescriptorPackages(
                        descriptor.Key,
                        descriptor.Value.Value.Dependencies,
                        resolvedPackages,
                        Detailed,
                        packageNameSelector,
                        lockedRepository == null ? null : lockedRepository.LockedPackages[string.Empty]
                        );
                }
            }
            foreach (var repository in repoToList)
                yield return new RepositoryPackages(repository, PackageManager.ListPackages(new[]{repository}, Query).OfType<PackageFoundResult>());
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
    public class DescriptorPackages : Info
    {
        public string DescriptorName { get; set; }
        public IEnumerable<Package> Packages { get; set; }

        public bool Detailed { get; private set; }

        public DescriptorPackages(string descriptorName,
            IEnumerable<PackageDependency> dependencies, 
            IEnumerable<IPackageInfo> resolvedPackages,
            bool detailed,
            Func<string,bool> nameSelector = null,
            IEnumerable<IPackageInfo> lockedPackages = null) : base()
        {
            Detailed = detailed;
            lockedPackages = lockedPackages ?? Enumerable.Empty<IPackageInfo>();
            DescriptorName = descriptorName == string.Empty
                                 ? "default scope"
                                 : descriptorName + " scope";
            
            var recursionDefender = new Stack<string>();
            var includeChildren = detailed || nameSelector != null;
            Packages = dependencies
                .Select(_ => 
                    CreatePackageInfo(_, resolvedPackages, lockedPackages, includeChildren, recursionDefender))
                .ToList();
            if (nameSelector != null)
                Packages = FilterByName(Packages, nameSelector);
        }

        
        IEnumerable<Package> FilterByName(IEnumerable<Package> packages, Func<string, bool> nameSelector)
        {
            return packages.Where(_ => Prune(_, nameSelector));
        }

        bool Prune(Package pack, Func<string, bool> nameSelector)
        {
            if (nameSelector(pack.PackageInfo.Name))
            {
                pack.Children = Truncated;
                return true;
            }
            List<Package> newChildren = new List<Package>();
            bool success = false;
            foreach(var child in pack.Children)
            {

                if (Prune(child, nameSelector))
                {
                    success = true;
                    newChildren.Add(child);
                }
            }
            pack.Children = newChildren;
            return success;
        }

        public static readonly ICollection<Package> Truncated = new List<Package>().AsReadOnly();
        Package CreatePackageInfo(PackageDependency dependency, IEnumerable<IPackageInfo> resolvedPackages, IEnumerable<IPackageInfo> lockedPackages, bool includeChildren, Stack<string> recursionDefender)
        {

            var packageInfo = new Package(
                resolvedPackages.FirstOrDefault(
                    pack => pack.Name.EqualsNoCase(dependency.Name)),
                dependency.ToString(),
                lockedPackages.Any(x => x.Name.EqualsNoCase(dependency.Name)));
            if (!includeChildren) return packageInfo;
            if (recursionDefender.ContainsNoCase(dependency.Name))
                packageInfo.Children = Truncated;
            else
            {
                recursionDefender.Push(dependency.Name);
                packageInfo.Children = (
                    packageInfo.PackageInfo.Dependencies.Select(
                    dep => CreatePackageInfo(dep, resolvedPackages, lockedPackages, includeChildren, recursionDefender)))
                    .ToList();
                recursionDefender.Pop();
            }
            return packageInfo;
        }

        public class Package
        {
            public Package(IPackageInfo packageInfo, string spec, bool locked)
            {
                PackageInfo = packageInfo;
                Spec = spec;
                Locked = locked;
                Children = new Package[0];
            }

            public IPackageInfo PackageInfo { get; set; }
            public string Spec { get; set; }
            public bool Locked { get; set; }
            public ICollection<Package> Children { get; set; }
        }
        public override string ToString()
        {
            var tree = new TreeRenderer();

            tree.PrintLine("─" + DescriptorName);
            PrintPackages(tree);

            return tree.Content;
        }

        void PrintPackages(TreeRenderer tree)
        {
            Packages.Select(_ => new Node<Package>(_, PrintPackage))
                .Cast<Node>()
                .Render(tree);
        }

        void PrintPackage(TreeRenderer tree, Package package)
        {
            var packageName = package.PackageInfo.Name + " " + package.PackageInfo.Version;
            if (package.PackageInfo.Title != null)
                packageName += " (" + package.PackageInfo.Title + ")";
            if (package.Locked)
                packageName += " [locked]";
            var dependsLine = "depends: " + package.Spec;

            tree.PrintLine(Detailed ? dependsLine : packageName);

            var childNodes = package.Children == Truncated
                                 ? new[] { new Node<string>("...", (t, val) => t.PrintLine(val)) }
                                 : package.Children.Select(_ => new Node<Package>(_, PrintPackage)).Cast<Node>();
            if (!Detailed) childNodes.Render(tree);
            else
            {
                
                new[]{new Node<string>(packageName,
                (t, val) =>
                {
                    t.PrintLine(val);
                    childNodes.Render(t);
                })}.Render(tree);
            }
        }


    }

    public abstract class Node
    {
        public abstract void Render(TreeRenderer tree);
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
    public class TreeRenderer
    {
        public string Content = string.Empty;
        public Stack<string> Prefix = new Stack<string>();
        string _currentPrefix;


        public TreeRenderer()
        {
            Prefix.Push(" ");

        }
        public void PrintLine(string text)
        {
            Content += Content.Length == 0 ? _currentPrefix + text : "\r\n" + _currentPrefix + text;
        }

        public void PrintChildren<T>(IEnumerable<T> nodes, Action<T> action)
        {
            var allNodes = nodes.ToList();
            if (allNodes.Count == 0) return;
            var oldPrefix = Prefix.Peek();

            _currentPrefix = oldPrefix + "├─";

            Prefix.Push(oldPrefix + (allNodes.Count > 1 ? "│ " : "  "));
            foreach (var node in allNodes.Take(allNodes.Count - 1))
            {
                _currentPrefix = oldPrefix + "├─";
                action(node);
            }
            Prefix.Pop();
            Prefix.Push(oldPrefix + "  ");
            _currentPrefix = oldPrefix + "└─";

            
            action(allNodes.Last());
            Prefix.Pop();
        }
    }
}