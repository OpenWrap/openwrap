using System.Collections.Generic;
using OpenFileSystem.IO;
using OpenWrap.Repositories.NuGet;

namespace OpenWrap.Commands.NuGet
{
    [Command(Noun="nuget", Verb="convert")]
    public class ConvertNuGet : AbstractCommand
    {
        IFile _nugetFile;
        IFile _destinationFile;
        protected IFileSystem FileSystem { get { return Services.Services.GetService<IFileSystem>(); } }
        [CommandInput(Position=0, IsRequired=true)]
        public string Path { get; set; }

        [CommandInput(Position = 1)]
        public string Destination { get; set; }

        public override IEnumerable<ICommandOutput> Execute()
        {
            return Either(VerifyInputs()).Or(ExecuteCore());
            
        }

        IEnumerable<ICommandOutput> ExecuteCore()
        {
            using(var nugetStream = _nugetFile.OpenRead())
            using(var wrapStream = _destinationFile.OpenWrite())
            {
                NuGetConverter.Convert(nugetStream, wrapStream);
            }
            yield return new GenericMessage("Package successfully converted.");
        }

        IEnumerable<ICommandOutput> VerifyInputs()
        {
            _nugetFile = FileSystem.GetFile(Path);
            if (!_nugetFile.Exists)
                yield return new Error("File '{0}' not found.", Path);

            _destinationFile = Destination == null
                                       ? FileSystem.GetFile(Destination)
                                       : _nugetFile.Parent.GetFile(_nugetFile.NameWithoutExtension + ".wrap");
        }
    }
}
