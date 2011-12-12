using NUnit.Framework;
using OpenFileSystem.IO;
using OpenWrap;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Commands.build_wrap.to_path
{
    public class directory_doesnt_exists : contexts.build_wrap
    {
        IDirectory path;

        public directory_doesnt_exists()
        {
            given_current_directory_repository(new CurrentDirectoryRepository());
            path = FileSystem.GetDirectory("unknown");
            given_descriptor(FileSystem.GetCurrentDirectory(),
                             new PackageDescriptor
                             {
                                 Name = "test",
                                 Version = "1.0.0.0".ToSemVer(),
                                 Build = { "custom;typename=" + typeof(NullPackageBuilder).AssemblyQualifiedName }
                             });
            when_executing_command("-path " + path.Path);
        }

        [Test]
        public void path_is_created()
        {
            path.Exists.ShouldBeTrue();
        }

        [Test]
        public void package_is_created_in_path()
        {
            path.GetFile("test-1.0.0+0.wrap").Exists.ShouldBeTrue();
        }
    }
}