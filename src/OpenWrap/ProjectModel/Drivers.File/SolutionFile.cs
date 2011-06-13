using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using OpenFileSystem.IO;
using OpenWrap.IO;

namespace OpenWrap.ProjectModel.Drivers.File
{
    public class SolutionFile : ISolution
    {
        const string GUID_REGEX = "[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}";
        const string NS_MSBUILD = "http://schemas.microsoft.com/developer/msbuild/2003";
        const string QUOTE = "\"";
        const string WS = "\\s*";
        readonly List<object> _content;
        readonly IFile _file;

        public SolutionFile(IFile inputFile, Version vsVersion)
        {
            _file = inputFile;
            _content = new List<object>
            {
                string.Empty,
                new SolutionFormatVersion(vsVersion),
                new SolutionVisualStudioVersion(SolutionConstants.Editions[vsVersion])
            };
        }

        SolutionFile(IFile inputFile)
        {
            _file = inputFile;
            _content = new List<object>();
            ParseContent();
        }

        public IEnumerable<IProject> AllProjects
        {
            get { return _content.OfType<IProject>(); }
        }

        public Version Version
        {
            get { return _content.OfType<SolutionFormatVersion>().Select(x => x.Version).FirstOrDefault(); }
        }

        public static SolutionFile Parse(IFile inputFile)
        {
            if (!inputFile.Exists) throw new FileNotFoundException("Solution file not found.", inputFile.Path);
            return new SolutionFile(inputFile);
        }

        public static string Quoted(string input)
        {
            return QUOTE + input + QUOTE;
        }

        public IProject AddProject(IFile projectFile)
        {
            using (var projectStream = projectFile.OpenRead())
            {
                var xmlDoc = XDocument.Load(new XmlTextReader(projectStream));
                var projectGuid = (from propertyGroup in xmlDoc.Descendants(XName.Get("PropertyGroup", NS_MSBUILD))
                                   from guid in propertyGroup.Descendants(XName.Get("ProjectGuid", NS_MSBUILD))
                                   select guid.Value).First();
                var projectTypeGuid = GetFromFileName(projectFile);

                var newProject = new Project
                {
                    Guid = new Guid(projectGuid),
                    Name = projectFile.NameWithoutExtension,
                    Path = projectFile.Path.MakeRelative(_file.Parent.Path),
                    Type = projectTypeGuid
                };
                var indexToInsert = _content.OfType<Project>().Any()
                                        ? _content.IndexOf(_content.OfType<Project>().Last()) + 1
                                        : _content.Count;
                _content.Insert(indexToInsert, newProject);
                return newProject;
            }
        }

        public void Save()
        {
            using (var writer = new StreamWriter(_file.OpenWrite(), Encoding.UTF8))
                foreach (var section in _content)
                    writer.WriteLine(section.ToString());
        }

        static string Group(string name, string val)
        {
            return string.Format("(?<{0}>{1})", name, val);
        }

        static string QuotedGuid(string captureGroup)
        {
            return Quoted("{" + Group(captureGroup, GUID_REGEX) + "}");
        }

        static string Text(string input)
        {
            var output = Regex.Escape(input);
            if (input == output) throw new ArgumentException("input does not need encoding");
            return output;
        }

        Guid GetFromFileName(IFile projectFile)
        {
            if (projectFile.Extension == ".csproj") return ProjectConstants.VisualCSharp;
            if (projectFile.Extension == ".vbproj") return ProjectConstants.VisualBasic;
            throw new ArgumentException("The project is not a known type.", "projectFile");
        }

        void ParseContent()
        {
            var lines = _file.ReadString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; i++)
            {
                _content.Add(
                    SolutionFormatVersion.TryParse(lines, ref i) ??
                    SolutionVisualStudioVersion.TryParse(lines, ref i) ??
                    Project.TryParse(lines, ref i) ??
                    (object)lines[i]
                    );
            }
        }

        class Project : IProject
        {
            public Guid Guid;
            public string Name;
            public string Path;
            public Guid Type;

            static readonly Regex _projectRegex = new Regex(
                Text("Project(") +
                QuotedGuid("projectTypeGuid") +
                Text(")") +
                WS + "=" + WS +
                Quoted(Group("name", ".*")) +
                WS + "," + WS +
                Quoted(Group("path", ".*")) +
                WS + "," + WS +
                QuotedGuid("projectGuid"));

            readonly List<string> _lines = new List<string>();
            

            public static Project TryParse(string[] lines, ref int index)
            {
                var match = _projectRegex.Match(lines[index]);
                if (match.Success == false) return null;
                var project = new Project
                {
                    Name = match.Groups["name"].Value,
                    Path = match.Groups["path"].Value,
                    Type = new Guid(match.Groups["projectTypeGuid"].Value),
                    Guid = new Guid(match.Groups["projectGuid"].Value)
                };


                for (index++; index < lines.Length; index++)
                {
                    if (lines[index] == "EndProject") break;
                    project._lines.Add(lines[index]);
                }
                return project;
            }

            public override string ToString()
            {
                return string.Format("Project(\"{{{0}}}\") = \"{1}\", \"{2}\", \"{{{3}}}\"\r\n{4}EndProject",
                                     Type.ToString().ToUpperInvariant(),
                                     Name,
                                     Path,
                                     Guid.ToString().ToUpperInvariant(),
                                     _lines.Aggregate(string.Empty, (input,line)=>input+line+"\r\n"));
            }
        }

        class SolutionFormatVersion
        {
            public readonly Version Version;
            static readonly Regex _solutionId = new Regex(@"Microsoft Visual Studio Solution File, Format Version (?<version>\d+\.\d+)");

            public SolutionFormatVersion(Version version)
            {
                Version = version;
            }

            public static SolutionFormatVersion TryParse(string[] lines, ref int index)
            {
                var match = _solutionId.Match(lines[index]);
                if (match.Success == false) return null;
                return new SolutionFormatVersion(match.Groups["version"].Value.ToVersion());
            }

            public override string ToString()
            {
                return string.Format("Microsoft Visual Studio Solution File, Format Version {0}.{1:00}", Version.Major, Version.Minor);
            }
        }

        class SolutionVisualStudioVersion
        {
            static readonly Regex _parser = new Regex(@"# Visual Studio (?<version>\S+)");
            readonly string Version;

            public SolutionVisualStudioVersion(string version)
            {
                Version = version;
            }

            public static SolutionVisualStudioVersion TryParse(string[] lines, ref int index)
            {
                var match = _parser.Match(lines[index]);
                return match.Success ? new SolutionVisualStudioVersion("Visual Studio " + match.Groups["version"].Value) : null;
            }

            public override string ToString()
            {
                return "# " + Version;
            }
        }
    }
}