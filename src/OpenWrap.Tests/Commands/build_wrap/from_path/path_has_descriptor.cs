using System;
using System.Linq;
using NUnit.Framework;
using OpenWrap;

using OpenWrap.Commands.Wrap;
using OpenWrap.PackageModel;
using OpenWrap.Testing;

namespace Tests.Commands.build_wrap.from_path
{
    public class path_has_descriptor : from_remote_path
    {
        public path_has_descriptor()
        {
            given_remote_project();
            given_descriptor(path_to_project, new PackageDescriptor { Name = "test", SemanticVersion = "1.0.0.0".ToSemVer(), Build = {"none"} });
            when_executing_command(string.Format("-from \"{0}\"", path_to_project.Path.FullPath));
        }

        [Test]
        public void command_successfull()
        {
            Results.ShouldHaveNoError();
        }

        [Test]
        public void package_details_are_output()
        {
            Results.OfType<PackageBuilt>().SingleOrDefault().ShouldNotBeNull()
                .File.ShouldBe(FileSystem.GetCurrentDirectory().GetFile("test-1.0.0+0.wrap"));
        }

    }
}