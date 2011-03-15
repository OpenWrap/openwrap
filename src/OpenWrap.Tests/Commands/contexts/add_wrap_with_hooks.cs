using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap;
using OpenWrap.Commands.contexts;
using OpenWrap.PackageManagement;
using OpenWrap.PackageModel;
using OpenWrap.Services;
using OpenWrap.Testing;

namespace Tests.Commands.contexts
{
    public class add_wrap_with_hooks : add_wrap_command
    {
        string Scope;
        protected List<ChangedCall> AddCalls = new List<ChangedCall>();

        protected List<ChangedCall> RemoveCalls = new List<ChangedCall>();
        protected List<ChangedCall> UpdateCalls = new List<ChangedCall>();


        public add_wrap_with_hooks()
        {
            given_scope(string.Empty);
        }
        protected void given_scope(string scope)
        {
            Scope = scope;

            var manager = ServiceLocator.GetService<IPackageManager>();
            manager.PackageAdded += (repo, name, version, packages) => Add(repo, name, scope, version, packages);

            manager.PackageRemoved += (repo, name, version, packages) => Removed(repo, name, scope, version, packages);
            manager.PackageUpdated += (repo, name, fromVersion, toVersion, packages) => Update(repo, name, scope, fromVersion, toVersion, packages);
        }

        IEnumerable<object> Removed(string repository, string name, string scope, Version version, IEnumerable<IPackageInfo> packages)
        {
            RemoveCalls.Add(new ChangedCall(repository, name, scope, version, packages));
            yield break;
        }

        IEnumerable<object> Update(string repository, string name, string scope, Version fromVersion, Version toVersion, IEnumerable<IPackageInfo> packages)
        {
            UpdateCalls.Add(new ChangedCall(repository, name, scope, fromVersion, toVersion, packages));
            yield break;

        }

        IEnumerable<object> Add(string repository, string name, string scope, Version version, IEnumerable<IPackageInfo> packages)
        {
            AddCalls.Add(new ChangedCall(repository, name, scope, version, packages));
            yield break;
        }
        protected void remove_hook_should_be_called(string expectedRepository, string expectedName, string expectedScope, Version expectedVersion)
        {

            RemoveCalls.FirstOrDefault(x => x.Repository == expectedRepository &&
                                            x.Name == expectedName &&
                                            x.Scope == expectedScope &&
                                            x.Version == expectedVersion).ShouldNotBeNull();
        }
        protected void add_hook_should_be_called(string expectedRepository, string expectedName, string expectedScope, Version expectedVersion)
        {
            AddCalls.FirstOrDefault(x => x.Repository == expectedRepository &&
                                         x.Name == expectedName &&
                                         x.Scope == expectedScope &&
                                         x.Version == expectedVersion).ShouldNotBeNull();
        }
        protected void update_hook_should_be_called(string expectedRepository, string expectedName, string expectedScope, string expectedFrom, string expectedTo)
        {
            UpdateCalls.FirstOrDefault(x => x.Repository == expectedRepository &&
                                            x.Name == expectedName &&
                                            x.Scope == expectedScope &&
                                            x.FromVersion == expectedFrom.ToVersion() &&
                                            x.ToVersion == expectedTo.ToVersion()).ShouldNotBeNull();
        }
        protected class ChangedCall
        {
            public string Repository { get; set; }
            public string Name { get; set; }
            public string Scope { get; set; }
            public Version Version { get; set; }
            public Version FromVersion { get; set; }
            public Version ToVersion { get; set; }
            public IEnumerable<IPackageInfo> Packages { get; set; }
            
            public ChangedCall(string repository, string name, string scope, Version fromVersion, Version toVersion, IEnumerable<IPackageInfo> packages)
            {
                Repository = repository;
                Name = name;
                Scope = scope;
                FromVersion = fromVersion;
                ToVersion = toVersion;
                Packages = packages;
            }
            public ChangedCall(string repository, string name, string scope, Version version, IEnumerable<IPackageInfo> packages)
            {
                Repository = repository;
                Name = name;
                Scope = scope;
                Version = version;
                Packages = packages;
            }
        }
    }
}