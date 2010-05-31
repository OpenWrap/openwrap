using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenWrap.Build.Services;

namespace OpenWrap.Commands.Wrap
{
    [Command(Verb = "add", Namespace = "wrap")]
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
            File.AppendAllText(_environment.DescriptorPath, GetDependsLine(), Encoding.UTF8);
            return null;
        }

        string GetDependsLine()
        {
            return "\r\ndepends " + Name + " " + (Version ?? string.Empty);
        }

        IEnumerable<ICommandResult> SyncWrapFileWithWrapDirectory()
        {
            return new SyncWrapCommand().Execute();
        }

        GenericError VerifyWrapFile()
        {
            return _environment.DescriptorPath != null ? null : new GenericError { Message = "Could not find a wrap descriptor in your path." };
        }

        ICommandResult VeryfyWrapRepository()
        {
            return _environment.ProjectRepository != null
                       ? null
                       : new GenericError
                       {
                           Message = string.Format("Directory 'wraps' not found on the path above the wrap file '{0}'.", _environment.DescriptorPath)
                       };
        }
    }
}