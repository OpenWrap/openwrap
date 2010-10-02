using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.Repositories;
using OpenWrap.Services;

namespace OpenWrap.Commands.Wrap
{
    [Command(Noun = "wrap", Verb = "clean", Description = "Clean all but the latest version of a wrap from the repository.")]
    public class CleanWrapCommand : AbstractCommand
    {
        [CommandInput(Position = 0)]
        public string Name { get; set; }

        [CommandInput(Name = "System", DisplayName = "System Only")]
        public bool SystemOnly { get; set; }

        [CommandInput(Name = "Project", DisplayName = "Project Only")]
        public bool ProjectOnly { get; set; }

        [CommandInput(Name = "All", DisplayName = "All")]
        public bool All { get; set; }

        internal IEnvironment Environment { get; private set; }

        public CleanWrapCommand()
            : this(WrapServices.GetService<IEnvironment>())
        {
        }

        public CleanWrapCommand(IEnvironment environment)
        {
            Environment = environment;
        }

        public override IEnumerable<ICommandOutput> Execute()
        {
            if (String.IsNullOrEmpty(Name) && !All)
            {
                yield return new GenericError("You must specify either a wrap name or the -All switch to run this command.");
                yield break;
            }

            if (SystemOnly && ProjectOnly)
            {
                yield return new GenericError("Cannot have both System and Project as parameters, they are mutually exclusive.");
                yield break;
            }

            var repositories = new List<IPackageRepository>();
            if (!SystemOnly) repositories.Add(Environment.ProjectRepository);
            if (!ProjectOnly) repositories.Add(Environment.SystemRepository);

            foreach (var message in RepositoriesCanBeCleaned(repositories))
            {
                yield return message;
                if (message is GenericError) yield break;
            }

            Func<IPackageRepository, IEnumerable<ICommandOutput>> deleter;
            if (All)
                deleter = DeleteAllPackagesFromRepo;
            else
                deleter = DeleteNamedPackagesFromRepo;

            foreach (var message in repositories.SelectMany(x => deleter(x)))
            {
                yield return message;
                if (message is GenericError) yield break;
            }

            yield return new Success();
        }

        IEnumerable<ICommandOutput> DeleteNamedPackagesFromRepo(IPackageRepository repository)
        {
            var packages = repository.PackagesByName[Name];

            if (packages.FirstOrDefault() == null)
            {
                yield return new GenericError("Package '{0}' does not exist in the {1}.",
                                              Name,
                                              repository.Name);
                yield break;
            }

            foreach (var message in DeleteAllButLatestVersion(packages))
                yield return message;
        }

        IEnumerable<ICommandOutput> DeleteAllPackagesFromRepo(IPackageRepository repository)
        {
            return repository.PackagesByName.SelectMany(x => DeleteAllButLatestVersion(x));
        }

        IEnumerable<ICommandOutput> RepositoriesCanBeCleaned(IEnumerable<IPackageRepository> repositories)
        {
            foreach (var repository in repositories)
            {
                if (!repository.CanDelete)
                {
                    yield return new GenericError("The {0} cannot be cleaned.",
                                                  repository.Name);
                    yield break;
                }
            }
        }

        IEnumerable<ICommandOutput> DeleteAllButLatestVersion(IEnumerable<IPackageInfo> packageList)
        {
            yield return new GenericMessage("Cleaning package {0} from {1}.",
                packageList.First().Name,
                packageList.First().Source.Name);

            var latestPackage = packageList.OrderByDescending(x => x.Version).First();
            foreach (var package in packageList.Where(x => x != latestPackage))
                package.Source.Delete(package);

            yield break;
        }
    }
}
