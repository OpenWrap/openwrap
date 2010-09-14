using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenFileSystem.IO;
using OpenWrap.Repositories;
using OpenWrap.Services;

namespace OpenWrap.Commands.Wrap
{
    [Command(Noun="wrap", Verb="publish", Description="Publishes a package to a remote reposiory.")]
    public class PublishWrapCommand : AbstractCommand
    {
        IPackageRepository _remoteRepository;
        IFile _packageFile;

        [CommandInput(IsRequired = true, Position = 0)]
        public string RemoteRepository { get; set; }

        [CommandInput(IsRequired = true, Position = 1)]
        public string Path { get; set; }

        protected IEnvironment Environment { get { return WrapServices.GetService<IEnvironment>(); } }
        protected IFileSystem FileSystem { get { return WrapServices.GetService<IFileSystem>(); } }
        
        public override IEnumerable<ICommandOutput> Execute()
        {
            return Either(Validate()).Or(ExecuteCore());
        }
        IEnumerable<ICommandOutput> Validate()
        {
            _remoteRepository = Environment.RemoteRepositories.FirstOrDefault(x => x.Name.Equals(RemoteRepository, StringComparison.OrdinalIgnoreCase));
            if (_remoteRepository == null)
                yield return new Errors.UnknownRemoteRepository(RemoteRepository);
            _packageFile = FileSystem.GetFile(Path);
            if (!_packageFile.Exists)
                yield return new Errors.FileNotFound(Path);
        }
        IEnumerable<ICommandOutput> ExecuteCore()
        {
            yield return new GenericMessage("Publishing package '{0}' to '{1}");
            using (var packageStream = _packageFile.OpenRead())
                _remoteRepository.Publish(_packageFile.Name, packageStream);
        }
    }

}
