using System.Linq;
using System.Xml;
using System.Xml.Linq;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenWrap.Testing;

namespace Tests.Commands.init_wrap
{
    public class init_for_all_projects : contexts.init_wrap
    {
        public init_for_all_projects()
        {
            given_csharp_project_file(@"project1.csproj");
            when_executing_command("-all");
        }
        [Test]
        public void command_succeeds()
        {
            Results.ShouldHaveNoError();
        }
        [Test]
        public void project_is_patched()
        {
            var projectFIle = Environment.CurrentDirectory.GetFile(@"project1.csproj");
            using (var stream = projectFIle.OpenRead())
            {
                var doc = XDocument.Load(new XmlTextReader(stream));

                doc.Document.Descendants(XName.Get("Import", MSBUILD_NS)).First().Attribute("Project").Value.ShouldContain("OpenWrap.CSharp.targets");
            }
        }

    }
}