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
    [Command(Noun = "wrap", Verb = "lock")]
    public class LockWrapCommand : AbstractCommand
    {
        readonly IEnvironment _env;
        readonly IPackageManager _packageManager;
        IPackageRepository _repository;

        [CommandInput(Position=0)]
        public IEnumerable<string> Name { get; set; }

        [CommandInput]
        public bool IgnoreDependencies { get; set; }

        public LockWrapCommand() : this(
            ServiceLocator.GetService<IEnvironment>(),
            ServiceLocator.GetService<IPackageManager>())
        {
            Name = Enumerable.Empty<string>();
        }
        public LockWrapCommand(IEnvironment env, IPackageManager packageManager)
        {
            _env = env;
            _packageManager = packageManager;
        }

        protected override IEnumerable<Func<IEnumerable<ICommandOutput>>> Validators()
        {
            yield return InProject;
            yield return RepositorySupportsLocking;
        }

        IEnumerable<ICommandOutput> InProject()
        {
            if (_env.ProjectRepository == null) yield return new NotInProject();
        }

        IEnumerable<ICommandOutput> RepositorySupportsLocking()
        {
            if (_env.ProjectRepository.Feature<ISupportLocking>() == null) yield return new LockingNotSupported(_env.ProjectRepository);
            _repository = _env.ProjectRepository;
        }

        protected override IEnumerable<ICommandOutput> ExecuteCore()
        {
            var lockingRepo = _repository.Feature<ISupportLocking>();
            var allPackages = _packageManager.ListProjectPackages(_env.Descriptor.Lock(_repository), _repository);
            if (Name.Any())
            {
                var notFound = Name.Where(_ => allPackages.FirstOrDefault(package => package.Name.EqualsNoCase(_)) == null).ToList();
                if (notFound.Any())
                    return notFound.Select(x => new PackageNotFound(x)).Cast<ICommandOutput>();
            }
            var currentPackages = GetPackagesToLock(_env.Descriptor, allPackages);

            lockingRepo.Lock(string.Empty, currentPackages);
            return OutputLock(currentPackages);
        }

        IEnumerable<IPackageInfo> GetPackagesToLock(IPackageDescriptor descriptor, IEnumerable<IPackageInfo> allPackages)
        {

            var currentPackages = allPackages;
            var names = Name.Any() ? Name : descriptor.Dependencies.Select(x => x.Name);

            if (names.Any())
            {
                var rootPackages = currentPackages.Where(_ => names.ContainsNoCase(_.Name));
                
                currentPackages = IgnoreDependencies
                    ? rootPackages
                    : currentPackages.AffectedPackages(rootPackages);
            }
            return currentPackages;
        }

        static IEnumerable<ICommandOutput> OutputLock(IEnumerable<IPackageInfo> currentPackages)
        {
            return currentPackages.Select(x => new PackageLocked(x)).Cast<ICommandOutput>();
        }
    }
}