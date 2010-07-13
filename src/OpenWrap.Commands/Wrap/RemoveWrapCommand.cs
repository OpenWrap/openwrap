using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OpenWrap.Dependencies;
using OpenWrap.IO;
using OpenWrap.Services;
using System.Text.RegularExpressions;

namespace OpenWrap.Commands.Wrap
{
    [Command(Verb = "remove", Noun = "wrap")]
    public class RemoveWrapCommand : ICommand
    {
        [CommandInput(IsRequired = true, Position = 0)]
        public string Name { get; set; }

        public IEnumerable<ICommandResult> Execute()
        {
            var dependency = FindDependencyByName();
            if (dependency == null)
            {
                yield return new GenericError("Dependency not found: " + Name);
                yield break;
            }

            Environment.Descriptor.Dependencies.Remove(dependency);
            RemoveDependsLineFromFile();
        }

        WrapDependency FindDependencyByName()
        {
            return Environment.Descriptor.Dependencies.FirstOrDefault(d => d.Name == Name);
        }

        void RemoveDependsLineFromFile()
        {
            var builder = new StringBuilder();
            using (var fileStream = Environment.Descriptor.File.OpenRead())
            {
                foreach (var line in RemoveDepends(GetLines(fileStream)))
                {
                    builder.AppendLine(line);                    
                }
            }

            using (var fileStream = Environment.Descriptor.File.Open(FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var writer = new StreamWriter(fileStream))
            {
                writer.Write(builder.ToString());
            }
        }

        IEnumerable<string> RemoveDepends(IEnumerable<string> lines)
        {
            // find start of the "depends ..." line
            var e = lines.GetEnumerator();
            var inDepends = false;
            var dependsLineStart = new Regex(@"depends\s+" + Regex.Escape(Name));
            while (e.MoveNext())
            {
                var line = e.Current;
                if (dependsLineStart.IsMatch(line))
                {
                    inDepends = true;
                    continue;
                }
                // check for continuing depends line (prefixed by whitespace)
                if (inDepends && Regex.IsMatch(line, @"^\s+")) continue;

                inDepends = false;
                yield return line;
            }
        }

        static IEnumerable<string> GetLines(Stream fileStream)
        {
            using (var reader = new StreamReader(fileStream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        static IEnvironment Environment
        {
            get { return WrapServices.GetService<IEnvironment>(); }
        }
    }
}
