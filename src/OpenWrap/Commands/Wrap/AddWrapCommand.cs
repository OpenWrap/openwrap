using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using OpenRasta.Wrap.Commands;
using OpenRasta.Wrap.Console;

namespace OpenWrap.Commands.Wrap
{
    [Command(Name = "add", Namespace = "wrap")]
    public class AddWrapCommand : ICommand
    {
        string _wrapFile;
        DirectoryInfo _wrapDirectory;

        [CommandInput(IsRequired = true, Position = 0)]
        public string Name { get; set; }

        [CommandInput(Position = 1)]
        public string Version { get; set; }

        public ICommandResult Execute()
        {
            Debugger.Launch();
            return FindWrapFile()
                    ?? FindWrapDirectory()
                    ?? AddInstructionToWrapFile()
                    ?? SyncWrapFileWithWrapDirectory();
        }

        ICommandResult AddInstructionToWrapFile()
        {
            File.AppendAllText(_wrapFile, GetDependsLine(), Encoding.UTF8);
            return null;
        }

        string GetDependsLine()
        {
            return "\r\ndepends " + Name + (Version == null ? string.Empty : Version);
        }

        ICommandResult FindWrapDirectory()
        {
            _wrapDirectory = new DirectoryInfo(Path.GetDirectoryName(_wrapFile))
                .SelfAndAncestors()
                .SelectMany(x => x.Directories("wraps"))
                .Where(x=>x != null)
                .FirstOrDefault();

            return _wrapDirectory != null
                       ? null
                       : new GenericError
                       {
                           Message = string.Format("Directory 'wraps' not found on the path above the wrap file '{0}'.", _wrapFile)
                       };
        }

        ICommandResult FindWrapFile()
        {
            _wrapFile = new DirectoryInfo(Environment.CurrentDirectory)
                .SelfAndAncestors()
                .SelectMany(x => x.Files("*.wrapdesc"))
                .FirstOrDefault();

            return _wrapFile != null ? null : new GenericError { Message = "Could not find a wrap descriptor in your path." };
        }

        ICommandResult SyncWrapFileWithWrapDirectory()
        {
            return new Success();
        }
    }
    public static class IOExtensions
    {
        public static IEnumerable<DirectoryInfo> SelfAndAncestors(this DirectoryInfo di)
        {
            do
            {
                yield return di;
                di = di.Parent;
            } while (di != null);
        }
        public static IEnumerable<string> Files(this DirectoryInfo di, string filePattern)
        {
            return System.IO.Directory.GetFiles(di.FullName, filePattern, SearchOption.TopDirectoryOnly);
        }
        public static string File(this DirectoryInfo di, string filePattern)
        {
            return di.Files(filePattern).FirstOrDefault();
        }
        public static DirectoryInfo Directory(this DirectoryInfo di, string directoryName)
        {
            return di.Directories(directoryName).FirstOrDefault();
        }
        public static IEnumerable<DirectoryInfo> Directories(this DirectoryInfo di, string directoryName)
        {
            return System.IO.Directory.GetDirectories(di.FullName, directoryName, SearchOption.TopDirectoryOnly)
                .Select(x => new DirectoryInfo(x));
        }
    }
}