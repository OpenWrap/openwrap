using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OpenFileSystem.IO;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;
using OpenWrap.Services;

namespace OpenWrap.Commands.Wrap
{
    [Command(Noun="wrap", Verb="publish", Description="Publishes a package to a remote reposiory.")]
    public class PublishWrapCommand : AbstractCommand
    {
        IPackageRepository _remoteRepository;

        Func<Stream> _packageStream;
        string _packageFileName;
        Version _packageVersion;
        string _packageName;

        [CommandInput(IsRequired = true, Position = 0)]
        public string RemoteRepository { get; set; }

        [CommandInput(Position = 1)]
        public string Path { get; set; }

        [CommandInput]
        public string Name { get; set; }

        protected IEnvironment Environment { get { return WrapServices.GetService<IEnvironment>(); } }
        protected IFileSystem FileSystem { get { return WrapServices.GetService<IFileSystem>(); } }
        
        public override IEnumerable<ICommandOutput> Execute()
        {
            return Either(ValidateInputs()).Or(ValidatePackageDoesntExist()).Or(ExecuteCore());
        }

        IEnumerable<ICommandOutput> ValidatePackageDoesntExist()
        {
            if (_remoteRepository.HasPackage(_packageName, _packageVersion.ToString()))
            {
                yield return new GenericError("The package '{0}' already exists. Please create a new version before uploading.", _packageFileName);
            }
        }

        IEnumerable<ICommandOutput> ValidateInputs()
        {
            _remoteRepository = Environment.RemoteRepositories.FirstOrDefault(x => x.Name.Equals(RemoteRepository, StringComparison.OrdinalIgnoreCase));
            if (_remoteRepository == null)
                yield return new Errors.UnknownRemoteRepository(RemoteRepository);
            if (Path != null)
            {
                var packageFile = FileSystem.GetFile(Path);
                if (!packageFile.Exists)
                    yield return new Errors.FileNotFound(Path);
                else
                {
                    _packageStream = () => packageFile.OpenRead();
                    _packageFileName = packageFile.Name;
                    var package = new CachedZipPackage(null, packageFile, null, null, false);
                    _packageName = package.Name;
                    _packageVersion = package.Version;
                }
            }
            else if (Name != null)
            {
                // get latest version of the Named package
                if (!Environment.CurrentDirectoryRepository.PackagesByName.Contains(Name))
                {
                    yield return new GenericError("No package named '{0}' was found.");
                    yield break;
                }
                var packageToCopy = Environment.CurrentDirectoryRepository.PackagesByName[Name].OrderByDescending(x=>x.Version).First();
                _packageStream = () => packageToCopy.Load().OpenStream();
                _packageFileName = packageToCopy.FullName + ".wrap";
                _packageName = packageToCopy.Name;
                _packageVersion = packageToCopy.Version;
            }
            else
            {
                yield return new GenericError("Please specify either a file path using the -Path input, or a name using -Name.");
            }
        }
        IEnumerable<ICommandOutput> ExecuteCore()
        {
            yield return new GenericMessage(String.Format("Publishing package '{0}' to '{1}'", _packageFileName, RemoteRepository));
            using (var packageStream = _packageStream())
                _remoteRepository.Publish(_packageFileName, packageStream);
        }
    }

}
