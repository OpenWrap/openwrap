using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.PackageManagement;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;
using OpenWrap.Runtime;
using OpenWrap.Services;

namespace OpenWrap.Commands.Wrap
{
    [Command(Noun="wrap", Verb="unlock")]
    public class UnlockWrapCommand : AbstractCommand
    {
        readonly IEnvironment _environment;
        readonly IPackageManager _packageManager;
        ISupportLocking _repositoryLock;

        [CommandInput(Position=0)]
        public IEnumerable<string> Name { get; set; }

        [CommandInput]
        public bool IgnoreDependencies { get; set; }

        public UnlockWrapCommand()
            : this(ServiceLocator.GetService<IEnvironment>(),
                   ServiceLocator.GetService<IPackageManager>())
        {
            Name = Enumerable.Empty<string>();
        }

        UnlockWrapCommand(IEnvironment environment, IPackageManager packageManager)
        {
            _environment = environment;
            _packageManager = packageManager;
        }

        protected override IEnumerable<Func<IEnumerable<ICommandOutput>>> Validators()
        {
            yield return InProject;
            yield return RepositorySupportsLocking;
        }

        IEnumerable<ICommandOutput> RepositorySupportsLocking()
        {
            _repositoryLock = _environment.ProjectRepository.Feature<ISupportLocking>();
            if (_repositoryLock == null)
                yield return new LockingNotSupported(_environment.ProjectRepository);
        }

        IEnumerable<ICommandOutput> InProject()
        {
            if (_environment.ProjectRepository == null)
                yield return new NotInProject();
        }

        protected override IEnumerable<ICommandOutput> ExecuteCore()
        {
            var packages = GetPackagesToUnlock();
            _repositoryLock.Unlock(string.Empty, packages);
            return packages.Select(_ => new PackageUnlocked(_)).Cast<ICommandOutput>();
        }

        IEnumerable<IPackageInfo> GetPackagesToUnlock()
        {
            var lockedPackages = _repositoryLock.LockedPackages[string.Empty];
            var allPackages = _packageManager.ListProjectPackages(_environment.Descriptor.Lock(lockedPackages), _environment.ProjectRepository);
            var names = Name.Any() ? Name : lockedPackages.Select(x => x.Name);

            if (!names.Any()) return Enumerable.Empty<IPackageInfo>();

            var rootPackages = lockedPackages.Where(_ => names.ContainsNoCase(_.Name));
            return IgnoreDependencies
                       ? rootPackages
                       : allPackages.AffectedPackages(rootPackages).Where(lockedPackages.Contains);

        }


    }
}
