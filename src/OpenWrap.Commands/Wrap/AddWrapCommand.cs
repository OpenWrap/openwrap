using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenWrap.Build.Services;
using OpenWrap.Dependencies;

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
            var dependLine = GetDependsLine();
            File.AppendAllText(_environment.Descriptor.Path,"\r\n" + dependLine, Encoding.UTF8);
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