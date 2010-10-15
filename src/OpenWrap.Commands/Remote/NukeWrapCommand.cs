using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.Configuration;
using OpenWrap.Repositories;
using OpenWrap.Services;

namespace OpenWrap.Commands.Remote
{
    [Command(Noun = "wrap", Verb = "nuke", Description = "Removes a wrap from a remote repository index.")]
    public class NukeWrapCommand : AbstractCommand
    {
        [CommandInput(IsRequired = true, Position = 0)]
        public string RemoteRepository { get; set; }

        [CommandInput(IsRequired = true, Position = 1)]
        public string WrapName { get; set; }

        [CommandInput(Position = 2)]
        public string Version { get; set; }

        [CommandInput]
        public bool Latest { get; set; }

        protected IEnvironment Environment { get { return Services.Services.GetService<IEnvironment>(); } }

        public override IEnumerable<ICommandOutput> Execute()
        {
            IPackageRepository repo = Environment.RemoteRepositories
                                  .FirstOrDefault(x => x.Name.Equals(RemoteRepository,
                                                                     StringComparison.OrdinalIgnoreCase));
            if (repo == null)
                yield return new Errors.UnknownRemoteRepository(RemoteRepository);

            var nukingRepo = repo as ISupportNuking;
            if (nukingRepo == null)
            {
                yield return new GenericError("The remote repository {0} does not support nuking.", RemoteRepository);
                yield break;
            }

            var packagesOfName = repo.PackagesByName[WrapName];
            if(!packagesOfName.Any())
            {
                yield return new GenericError("The remote repository {0} does not contain any package called {1}.", 
                    RemoteRepository,
                    WrapName);
                yield break;
            }
            var packageToNuke = packagesOfName
                .Where(x => x.Version.ToString().Equals(Version))
                .FirstOrDefault();
            
            if(packageToNuke == null)
            {
                yield return new GenericError("The package {0} does not have a version {1} in the remote repository {2}.", 
                    WrapName,
                    Version,
                    RemoteRepository);
                yield break;
            }

            nukingRepo.Nuke(packageToNuke);

            yield return new GenericMessage("{0} {1} was successfully nuked from the remote repository {2}.",
                WrapName,
                Version,
                RemoteRepository);
            yield return new Success();
        }

    }
}
