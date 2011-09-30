using System;
using System.Linq;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenWrap;

using OpenWrap.Commands.Wrap;
using OpenWrap.PackageModel;
using OpenWrap.Testing;

namespace Tests.Commands.build_wrap.from_path
{
    public class from_remote_path : contexts.build_wrap
    {
        protected IDirectory path_to_project;

        protected void given_remote_project()
        {
            path_to_project = FileSystem.CreateTempDirectory();
        }
    }

    public class path_has_descriptor : from_remote_path
    {
        public path_has_descriptor()
        {
            given_remote_project();
            given_descriptor(path_to_project, new PackageDescriptor { Name = "test", Version = "1.0.0.0".ToVersion(), Build = {"none"} });
            when_executing_command("-from " + path_to_project.Path.FullPath);
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
                .PackageFile.ShouldBe(FileSystem.GetCurrentDirectory().GetFile("test-1.0.0.0.wrap"));
        }

    }
}