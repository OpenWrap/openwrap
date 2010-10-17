using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.Repositories;
using OpenWrap.Services;

namespace OpenWrap.Commands.Wrap
{
    [Command(Noun = "wrap", Verb = "clean", Description = "Clean all but the latest version of a wrap from the repository.")]
    public class CleanWrapCommand : WrapCommand
    {
        List<Func<IEnumerable<ICommandOutput>>> _cleanOperations = new List<Func<IEnumerable<ICommandOutput>>>();

        [CommandInput(Position = 0, DisplayName="The name of the package to clean")]
        public string Name { get; set; }

        [CommandInput(DisplayName = "Cleans the System repository")]
        public bool System { get; set; }

        bool? _project;

        [CommandInput(DisplayName = "Cleans the current project's repository")]
        public bool Project
        {
            get { return _project ?? false; }
            set { _project = value; }
        }

        public override IEnumerable<ICommandOutput> Execute()
        {
            return Either(VerifyInputs()).Or(ExecuteCore());
        }

        IEnumerable<ICommandOutput> ExecuteCore()
        {
            foreach (var m in _cleanOperations.SelectMany(x => x())) yield return m;
            
        }
        bool IncludeProject
        {
            get { return (_project == null && System == false) || (_project != null && _project.Value); }
        }
        IEnumerable<ICommandOutput> VerifyInputs()
        {
            int countWithMatchingName = 0;
            if (IncludeProject)
            {
                yield return TryAddRepository("Project repository", Environment.ProjectRepository, GetProjectPackages());
                if (Name != null && Environment.ProjectRepository != null)
                    countWithMatchingName = Environment.ProjectRepository.PackagesByName[Name].Count();
            }
            if (System)
            {
                countWithMatchingName += Environment.SystemRepository.PackagesByName[Name].Count();
                yield return TryAddRepository("System repository", Environment.SystemRepository, GetLastVersionOfSystemRepository());
            }
            if (Name != null && countWithMatchingName == 0)
                yield return new GenericError("Cound not find a package called '{0}'.", Name);

        }

        IEnumerable<IPackageInfo> GetLastVersionOfSystemRepository()
        {
            return from packageByName in Environment.SystemRepository.PackagesByName
                   select packageByName.OrderByDescending(x=>x.Version).First();
        }

        IEnumerable<IPackageInfo> GetProjectPackages()
        {
            return PackageManager.TryResolveDependencies(Environment.Descriptor, new[] { Environment.ProjectRepository }).Dependencies.Select(x => x.Package);
        }

        ICommandOutput TryAddRepository(string repositoryName, IPackageRepository repository, IEnumerable<IPackageInfo> packagesToKeep)
        {
            if (repository == null)
                return new GenericError("Repository '{0}' not found.", repositoryName);
            var repo = repository as ISupportCleaning;
            if (repo == null)
                return new GenericError("Repository '{0}' does not support cleaning.", repositoryName);

            if (Name != null)
            {
                packagesToKeep = (
                                         from packageByName in repository.PackagesByName
                                         where !packageByName.Key.Equals(Name, StringComparison.OrdinalIgnoreCase)
                                         from package in packageByName
                                         select package
                                 )
                                 .Concat(packagesToKeep.Where(x => x.Name.Equals(Name, StringComparison.OrdinalIgnoreCase)))
                                .ToList();
            }

            _cleanOperations.Add(()=>CleanRepository(repo, packagesToKeep));
            return null;
        }

        IEnumerable<ICommandOutput> CleanRepository(ISupportCleaning repository, IEnumerable<IPackageInfo> packagesToKeep)
        {
            foreach (var package in repository.Clean(packagesToKeep))
            {
                if (package.Success)
                    yield return new GenericMessage("Package '{0}' removed.", package.Package.FullName);
                else
                    yield return new GenericMessage("Package '{0}' could not be removed, possibly because a file is still in use. If you use Visual Studio, try closing it and retrying.", package.Package.FullName);
            }
            repository.RefreshAnchors(PackageManager.TryResolveDependencies(Environment.Descriptor, new[] { Environment.ProjectRepository }));
        }
    }
}
