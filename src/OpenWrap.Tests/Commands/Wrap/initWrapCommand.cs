using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using initWrap_specs.context;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenWrap.Commands.Wrap;
using OpenWrap.Testing;
using OpenWrap.Tests.Commands.context;

namespace initWrap_specs
{
    public class init_for_all_projects : init_wrap
    {
        public init_for_all_projects()
        {
            given_csharp_project_file(@"project1.csproj");
            when_executing_command("-all");
        }
        [Test]
        public void the_command_succeeds()
        {
            Results.ShouldHaveAll(x => x.Success);
        }
        [Test]
        public void the_project_is_patched()
        {
            var projectFIle = Environment.CurrentDirectory.GetFile(@"project1.csproj");
            using (var stream = projectFIle.OpenRead())
            {
                var doc = XDocument.Load(new XmlTextReader(stream));
                
                doc.Document.Descendants(XName.Get("Import", MSBUILD_NS)).First().Attribute("Project").Value
                    .ShouldContain("OpenWrap.CSharp.targets");
            }
        }

    }
    namespace context
    {
        public abstract class init_wrap : command_context<InitWrapCommand>
        {
            protected const string MSBUILD_NS = "http://schemas.microsoft.com/developer/msbuild/2003";

            protected void given_csharp_project_file(string projectPath)
            {
                var file = Environment.CurrentDirectory.GetFile(projectPath);
                using (var fileStream = file.OpenWrite())
                {
                    var xmlDoc = Encoding.UTF8.GetBytes(
                            @"
<Project ToolsVersion=""3.5"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
    <Import Project=""$(MSBuildToolsPath)\Microsoft.CSharp.targets"" />
</Project>
");
                    fileStream.Write(xmlDoc, 0, xmlDoc.Length);
                }
            }
        }
    }
}
