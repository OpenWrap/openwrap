using System.Text;
using OpenFileSystem.IO;
using OpenWrap.Commands.Wrap;

namespace Tests.Commands.contexts
{
    public abstract class init_wrap : command<InitWrapCommand>
    {
        public init_wrap()
        {
            given_system_package("sharpziplib", "0.85.1");
            given_system_package("openfilesystem", "1.0.0");
            given_system_package("openwrap", "1.0.0", "depends: openfilesystem", "depends: sharpziplib");                
        }
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