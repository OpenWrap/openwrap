using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Commands.Messages;
using OpenWrap.Commands.Wrap;
using OpenWrap.PackageModel;
using OpenWrap.Testing;

namespace Tests.Commands.build_wrap.from_path
{
    public class path_does_not_exist : from_remote_path
    {
            public path_does_not_exist()
            {
                path_to_project = FileSystem.GetTempDirectory().GetDirectory("doesnotexist");
                when_executing_command("-from " + path_to_project.Path.FullPath);
            }

            [Test]
            public void command_fails()
            {
                Results.ShouldHaveOne<DirectoryNotFound>()
                    .Directory.ShouldBe(path_to_project);
            }
    }
}