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

        public bool OpenWrapAddInEnabled
        {
            get
            { 
                string progId = Version.Major == 10 ? SolutionConstants.ADD_IN_PROGID_2010 : SolutionConstants.ADD_IN_PROGID_2008;
                var global = _content.OfType<Global>().FirstOrDefault();
                if (global == null) return false;
                var addinSection = global.Sections.OfType<ExtensibilityAddInsGlobalSection>().FirstOrDefault();
                if (addinSection == null) return false;
                return addinSection.AddIns.Any(x => x.ProgId == progId);
            }
            set
            {
                string progId = Version.Major == 10 ? SolutionConstants.ADD_IN_PROGID_2010 : SolutionConstants.ADD_IN_PROGID_2008;
                if (value == false && !OpenWrapAddInEnabled) return;
                if (value == false)
                {
                    _content.OfType<Global>().First().Sections.OfType<ExtensibilityAddInsGlobalSection>().First().AddIns.RemoveAll(x => x.ProgId == progId);
                    return;
                }

                var global = _content.OfType<Global>().FirstOrDefault();
                if (global == null) _content.Add(global = new Global());
                var addinSection = global.Sections.OfType<ExtensibilityAddInsGlobalSection>().FirstOrDefault();
                if (addinSection == null) global.Sections.Add(addinSection = new ExtensibilityAddInsGlobalSection());
                addinSection.AddIns.Add(new AddIn(progId, true, SolutionConstants.ADD_IN_NAME, SolutionConstants.ADD_IN_DESCRIPTION));
            }
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
                    Global.TryParse(lines, ref i) ??
                    (object)lines[i]
                    );
            }
        }
        class Global
        {
            public List<GlobalSection> Sections = new List<GlobalSection>();

            public static Global TryParse(string[] lines, ref int index)
            {
                if (lines[index] != "Global") return null;
                var global = new Global();
                for(index++; index < lines.Length; index++)
                {
                    if (lines[index] == "EndGlobal") break;
                    var section = GlobalSection.TryParse(lines, ref index);
                    if (section == null) continue;
                    global.Sections.Add(section);
                }
                return global;
            }

            public override string ToString()
            {
                return "Global\r\n" +
                       Sections.Select(x => x.ToString()).JoinString("\r\n") + "\r\n"
                       + "EndGlobal";
            }
        }
        class GlobalSection
        {
            public virtual string Name { get; set; }
            public virtual GlobalSectionInitialization Type { get; set; }
            static Regex _sectionParser = new Regex(Text("\tGlobalSection(") + Group("name", ".*") + Text(") = ") + Group("type", ".*"));
            protected const string EndInstruction = "\tEndGlobalSection";

            protected GlobalSection(string name, GlobalSectionInitialization type, List<string> sectionLines)
            {
                Name = name;
                Type = type;
                Lines = sectionLines;
            }

            public static GlobalSection TryParse(string[] lines, ref int index)
            {
                var match = _sectionParser.Match(lines[index]);
                if (match.Success == false) return null;
                var type = match.Groups["type"].Value;
                var name = match.Groups["name"].Value;

                var sectionLines = new List<string>();
                for(index++; index < lines.Length; index++)
                {
                    if (lines[index] == EndInstruction) break;
                    sectionLines.Add(lines[index]);

                }
                var globalSectionInitialization = (GlobalSectionInitialization)Enum.Parse(typeof(GlobalSectionInitialization), type, true);
                var section = name == "ExtensibilityAddIns"
                    ? new ExtensibilityAddInsGlobalSection(name, globalSectionInitialization, sectionLines)
                    : new GlobalSection(name, globalSectionInitialization, sectionLines);
                return section;
            }

            public virtual IEnumerable<string> Lines { get; private set; }
            public override string ToString()
            {
                return BeginInstruction + "\r\n" + Lines.JoinString("\r\n") +  "\r\n" + EndInstruction;
            }

            protected string BeginInstruction
            {
                get { return string.Format("\tGlobalSection({0}) = {1}", Name, Type.ToString()); }
            }
        }
        class ExtensibilityAddInsGlobalSection : GlobalSection
        {
            Regex _addin = new Regex(WS + Group("progid", "\\S+") + WS + Text("=") + WS +
                                     Group("connected", "(0|1)") + Text(";") +
                                     Group("description", "[^;]") + Text(";") +
                                     Group("name", ".*"));

            public ExtensibilityAddInsGlobalSection()
                : base("ExtensibilityAddIns", GlobalSectionInitialization.postSolution, new List<string>())
            {
                
            }
            public ExtensibilityAddInsGlobalSection(string name, GlobalSectionInitialization type, List<string> sectionLines)
                : base(name,type,sectionLines)
            {
                foreach(var line in sectionLines)
                {
                    var match = _addin.Match(line);
                    if (match.Success == false) throw new InvalidOperationException("Cannot parse addin section");

                    AddIns.Add(new AddIn(match.Groups["progid"].Value, match.Groups["connected"].Value == "1", match.Groups["name"].Value, match.Groups["description"].Value));

                }
            }
            public override IEnumerable<string> Lines
            {
                get
                {
                    return AddIns.Select(x => x.ToString());
                }
            }

            public List<AddIn> AddIns { get; set; }
        }
        class AddIn
        {
            public string ProgId { get; set; }
            public bool Connected { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }

            public AddIn(string progId, bool connected, string name, string description)
            {
                ProgId = progId;
                Connected = connected;
                Name = name;
                Description = description;
            }
            public override string ToString()
            {
                return string.Format("\t\t{0} = {1};{2};{3}", ProgId, Connected ? "1" : "0", Description, Name);
            }
        }

        // ReSharper disable InconsistentNaming
        enum GlobalSectionInitialization
        {
            preSolution,
            postSolution
        }
        // ReSharper restore InconsistentNaming

        class Project : IProject
        {
            public Guid Guid;
            public string Name;
            public string Path;
            public Guid Type;

            static readonly Regex _projectRegex = new Regex(
                Text("Project(") + QuotedGuid("projectTypeGuid") + Text(")") +
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