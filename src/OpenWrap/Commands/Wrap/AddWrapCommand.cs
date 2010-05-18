using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using OpenWrap.Build.Services;
using OpenWrap.Commands;

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

        public ICommandResult Execute()
        {
            _environment = WrapServices.GetService<IEnvironment>();
            return VerifyWrapFile()
                    ?? VeryfyWrapRepository()
                    ?? AddInstructionToWrapFile()
                    ?? SyncWrapFileWithWrapDirectory();
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

        ICommandResult VeryfyWrapRepository()
        {
           return _environment.ProjectRepository != null
                       ? null
                       : new GenericError
                       {
                           Message = string.Format("Directory 'wraps' not found on the path above the wrap file '{0}'.", _environment.DescriptorPath)
                       };
        }

        ICommandResult VerifyWrapFile()
        {   
            return _environment.DescriptorPath != null ? null : new GenericError { Message = "Could not find a wrap descriptor in your path." };
        }

        ICommandResult SyncWrapFileWithWrapDirectory()
        {
            return new SyncWrapCommand().Execute();
        }
    }
}