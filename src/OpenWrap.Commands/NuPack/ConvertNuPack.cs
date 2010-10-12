using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenFileSystem.IO;
using OpenWrap.Commands;
using OpenWrap.Repositories.NuPack;
using OpenWrap.Services;

namespace OpenWrap.Tests.Commands.NuPack
{
    [Command(Noun="nupack", Verb="convert")]
    public class ConvertNuPack : AbstractCommand
    {
        IFile _nuPackFile;
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
            using(var nuPackStream = _nuPackFile.OpenRead())
            using(var wrapStream = _destinationFile.OpenWrite())
            {
                NuPackConverter.Convert(nuPackStream, wrapStream);
            }
            yield return new GenericMessage("Package successfully converted.");
        }

        IEnumerable<ICommandOutput> VerifyInputs()
        {
            _nuPackFile = FileSystem.GetFile(Path);
            if (!_nuPackFile.Exists)
                yield return new GenericError("File '{0}' not found.", Path);

            _destinationFile = Destination == null
                                       ? FileSystem.GetFile(Destination)
                                       : _nuPackFile.Parent.GetFile(_nuPackFile.NameWithoutExtension + ".wrap");
        }
    }
}
