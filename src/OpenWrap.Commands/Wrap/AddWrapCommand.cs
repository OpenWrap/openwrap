using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenWrap.Build.Services;
using OpenWrap.Dependencies;
using OpenWrap.IO;

namespace OpenWrap.Commands.Wrap
{
    [Command(Verb = "add", Noun = "wrap")]
    public class AddWrapCommand : ICommand
    {
        IEnvironment _environment;

        [CommandInput(IsRequired = true, Position = 0)]
        public string Name { get; set; }

        [CommandInput(Position = 1)]
        public string Version { get; set; }

        [CommandInput]
        public bool LocalOnly { get; set; }

        [CommandInput]
        public bool SystemOnly { get; set; }
        
        

        public IEnumerable<ICommandResult> Execute()
        {
            _environment = WrapServices.GetService<IEnvironment>();
            yield return VerifyWrapFile();
            yield return VeryfyWrapRepository();
            yield return AddInstructionToWrapFile();
            foreach (var nestedResult in SyncWrapFileWithWrapDirectory())
                yield return nestedResult;
        }

        ICommandResult AddInstructionToWrapFile()
        {
            // TODO: Make the environment descriptor separate from reader/writer,
            // and remove the File property on it.
            var dependLine = GetDependsLine();
            using(var fileStream = _environment.Descriptor.File.OpenWrite())
            using (var textWriter = new StreamWriter(fileStream, Encoding.UTF8))
                textWriter.WriteLine("\r\n" + dependLine);
            new WrapDependencyParser().Parse(dependLine, _environment.Descriptor);
            return null;
        }

        string GetDependsLine()
        {
            return "depends " + Name + " " + (Version ?? string.Empty);
        }

        IEnumerable<ICommandResult> SyncWrapFileWithWrapDirectory()
        {
            return new SyncWrapCommand().Execute();
        }

        GenericError VerifyWrapFile()
        {
            return _environment.Descriptor != null ? null : new GenericError { Message = "Could not find a wrap descriptor in your path." };
        }

        ICommandResult VeryfyWrapRepository()
        {
            return _environment.ProjectRepository != null
                       ? null
                       : new GenericError
                       {
                           Message = string.Format("Directory 'wraps' not found on the path above the wrap file '{0}'.", _environment.Descriptor)
                       };
        }
    }
}