using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using OpenFileSystem.IO;
using OpenWrap.Commands.Errors;
using OpenWrap.Commands.Messages;
using OpenWrap.PackageManagement.Packages;
using OpenWrap.Repositories;

namespace OpenWrap.Commands.Wrap
{
    [Command(Noun = "wrap", Verb = "publish", Description = "Publishes a package to a remote reposiory.")]
    public class PublishWrapCommand : WrapCommand
    {
        ISupportAuthentication _authenticationSupport;

        string _packageFileName;
        string _packageName;
        Func<Stream> _packageStream;
        SemanticVersion _packageVersion;
        IPackageRepository _remoteRepository;
        NetworkCredential _credentials;

        [CommandInput]
        public string Name { get; set; }

        [CommandInput(Position = 1)]
        public string Path { get; set; }

        [CommandInput]
        public string Password { get; set; }

        [CommandInput(IsRequired = true, Position = 0)]
        public string Remote { get; set; }

        [CommandInput]
        public string Username { get; set; }

        protected override IEnumerable<ICommandOutput> ExecuteCore()
        {
            yield return new Info(String.Format("Publishing package '{0}' to '{1}'", _packageFileName, Remote));

            var credentialCookie =_credentials != null
                                      ? _authenticationSupport.WithCredentials(_credentials)
                                      : null;
            try
            {
                using (var publisher = _remoteRepository.Feature<ISupportPublishing>().Publisher())
                using (var packageStream = _packageStream())
                    publisher.Publish(_packageFileName, packageStream);
            }
            finally
            {
                if (credentialCookie != null) credentialCookie.Dispose();
            }
        }

        protected override IEnumerable<Func<IEnumerable<ICommandOutput>>> Validators()
        {
            yield return ValidateInputs;
            yield return PackageDoesNotAlreadyExist;
        }

        IEnumerable<ICommandOutput> ValidateInputs()
        {
            // TODO: HACK HACK HACK
            var namedRepository = Remotes.PublishRepositories(Remote).SelectMany(_ => _).FirstOrDefault();

            if (namedRepository == null)
            {
                yield return new UnknownRemoteName(Remote);
                foreach (var _ in HintRemoteRepositories()) yield return _;
                yield break;
            }

            if ((Username != null && Password == null) || (Username == null && Password != null))
            {
                yield return new IncompleteCredentials();
                yield break;
            }

            bool hasAuth = Username != null && Password != null;
            if (hasAuth) _credentials = new NetworkCredential(Username, Password);
            _authenticationSupport = namedRepository.Feature<ISupportAuthentication>();
            if (hasAuth && _authenticationSupport == null)
            {
                yield return new RemoteAuthenticatioNotSupported(namedRepository);
                yield break;
            }

            _authenticationSupport = _authenticationSupport ?? new NullAuthentication();

            //_repositories = namedRepository.
            _remoteRepository = namedRepository;
            var publishingRepo = _remoteRepository.Feature<ISupportPublishing>();

            if (publishingRepo == null)
            {
                yield return new Error("Repository '{0}' doesn't support publishing.", namedRepository.Name);
                yield break;
            }
            if (Path != null)
            {
                var packageFile = FileSystem.GetFile(Path);
                if (!packageFile.Exists)
                {
                    yield return new FileNotFound(Path);
                    yield break;
                }
                _packageStream = packageFile.OpenRead;
                _packageFileName = packageFile.Name;
                // TODO: This looks iffy at best
                var package = new ZipFilePackage(packageFile);
                _packageName = package.Name;
                _packageVersion = package.SemanticVersion;
            }
            else if (Name != null)
            {
                // get latest version of the Named package
                if (!HostEnvironment.CurrentDirectoryRepository.PackagesByName.Contains(Name))
                {
                    yield return new Error("No package named '{0}' was found.", Name);
                    yield break;
                }
                var packageToCopy = HostEnvironment.CurrentDirectoryRepository.PackagesByName[Name].OrderByDescending(x => x.SemanticVersion).First();
                _packageStream = () => packageToCopy.Load().OpenStream();
                _packageFileName = packageToCopy.FullName + ".wrap";
                _packageName = packageToCopy.Name;
                _packageVersion = packageToCopy.SemanticVersion;
            }
            else
            {
                yield return new Error("Please specify either a file path using the -Path input, or a name using -Name.");
            }
        }

        IEnumerable<ICommandOutput> PackageDoesNotAlreadyExist()
        {
            if (_remoteRepository.HasPackage(_packageName, _packageVersion.ToString()))
                yield return new Error("The package '{0}' already exists. Please create a new version before uploading.", _packageFileName);
        }
    }

    public class RemoteAuthenticatioNotSupported : Error
    {
        public IPackageRepository Repository { get; set; }

        public RemoteAuthenticatioNotSupported(IPackageRepository repository)
            : base("Remote repository '{0}' does not support authentication.", repository.Name)
        {
            Repository = repository;
        }
    }
}