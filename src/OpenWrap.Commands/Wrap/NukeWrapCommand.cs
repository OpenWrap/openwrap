using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Repositories;

namespace OpenWrap.Commands.Wrap
{
    [Command(Noun = "wrap", Verb = "nuke", Description = "Removes a wrap from a remote repository index.")]
    public class NukeWrapCommand : WrapCommand
    {
        [CommandInput(IsRequired = true, Position = 0, IsValueRequired = false)]
        public string Remote { get; set; }

        [CommandInput(IsRequired = true, Position = 1)]
        public string Name { get; set; }

        [CommandInput(Position = 2)]
        public string Version { get; set; }


        protected IEnvironment Environment { get { return Services.Services.GetService<IEnvironment>(); } }

        public override IEnumerable<ICommandOutput> Execute()
        {
            IPackageRepository repo = GetRemoteRepository(Remote);
            if (repo == null)
            {
                yield return new Errors.UnknownRemoteRepository(Remote);
                foreach (var m in HintRemoteRepositories()) yield return m;
                yield break;
            }
            var nukingRepo = repo as ISupportNuking;
            if (nukingRepo == null)
            {
                yield return new Error("The remote repository {0} does not support nuking.", Remote);
                yield break;
            }

            var packagesOfName = repo.PackagesByName[Name];
            if(!packagesOfName.Any())
            {
                yield return new Error("The remote repository {0} does not contain any package called {1}.", 
                    Remote,
                    Name);
                yield break;
            }
            var packageToNuke = packagesOfName
                .Where(x => x.Version.ToString().Equals(Version))
                .FirstOrDefault();
            
            if(packageToNuke == null)
            {
                yield return new Error("The package {0} does not have a version {1} in the remote repository {2}.", 
                    Name,
                    Version,
                    Remote);
                yield break;
            }

            nukingRepo.Nuke(packageToNuke);

            yield return new GenericMessage("{0} {1} was successfully nuked from the remote repository {2}.",
                Name,
                Version,
                Remote);
            yield return new Success();
        }

    }
}
