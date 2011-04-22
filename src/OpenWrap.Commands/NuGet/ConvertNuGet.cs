using System;
using System.Collections.Generic;
using OpenFileSystem.IO;
using OpenWrap.Repositories.NuGet;
using OpenWrap.Services;

namespace OpenWrap.Commands.NuGet
{
    [Command(Noun = "nuget", Verb = "convert")]
    public class ConvertNuGet : AbstractCommand
    {
        IFile _destinationFile;
        IFile _nugetFile;

        [CommandInput(Position = 1)]
        public string Destination { get; set; }

        [CommandInput(Position = 0, IsRequired = true)]
        public string Path { get; set; }

        protected IFileSystem FileSystem
        {
            get { return ServiceLocator.GetService<IFileSystem>(); }
        }

        protected override IEnumerable<ICommandOutput> ExecuteCore()
        {
            using (var nugetStream = _nugetFile.OpenRead())
            using (var wrapStream = _destinationFile.OpenWrite())
            {
                NuGetConverter.Convert(nugetStream, wrapStream);
            }
            yield return new GenericMessage("Package successfully converted.");
        }

        protected override IEnumerable<Func<IEnumerable<ICommandOutput>>> Validators()
        {
            yield return VerifyInputs;
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