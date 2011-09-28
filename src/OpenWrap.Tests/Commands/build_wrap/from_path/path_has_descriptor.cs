using System;
using System.Linq;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenWrap;

using OpenWrap.Commands.Wrap;
using OpenWrap.PackageModel;
using OpenWrap.PackageModel.Serialization;
using OpenWrap.Testing;

namespace Tests.Commands.build_wrap.from_path
{
    public class from_remote_path : command<BuildWrapCommand>
    {
        protected ITemporaryDirectory path_to_project;

        protected void given_remote_project()
        {
            path_to_project = FileSystem.CreateTempDirectory();
        }

        protected void given_remote_descriptor(PackageDescriptor packageDescriptor)
        {
            using(var descriptor = path_to_project.GetFile(packageDescriptor.Name + ".wrapdesc").OpenWrite())
                new PackageDescriptorReaderWriter().Write(packageDescriptor, descriptor);
        }
    }

    public class path_has_descriptor : from_remote_path
    {
        public path_has_descriptor()
        {
            given_remote_project();
            given_remote_descriptor(new PackageDescriptor { Name = "test", Version = "1.0.0.0".ToVersion(), Build = {"none"} });
            when_executing_command("-from " + path_to_project.Path.FullPath);
        }

        [Test]
        public void command_successfull()
        {
            Results.ShouldHaveNoError();
        }

        [Test]
        public void pacakge_is_output()
        {
            Results.OfType<PackageBuilt>().SingleOrDefault().ShouldNotBeNull()
                .PackageFile.ShouldBe(FileSystem.GetCurrentDirectory().GetFile("test-1.0.0.0.wrap"));
        }
    }
}