using System.IO;
using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Commands.Wrap;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Commands.build_wrap.providers.files
{
    [TestFixture("test.txt", "files; file=bin-net35->test.txt")]
    [TestFixture("test.txt", "files; file = bin-net35 -> test.txt ")]
    public class from_existing_files : command<BuildWrapCommand>
    {
        readonly string _filePath;

        public from_existing_files(string filePath, string instruction)
        {
            _filePath = filePath;
            given_current_directory_repository(new CurrentDirectoryRepository());
            Environment.Descriptor.Name = "package";
            Environment.Descriptor.Version = "1.0.0".ToVersion();
            Environment.Descriptor.Build.Add(instruction);

            given_file(filePath, new MemoryStream(new byte[]{0x0}));

            when_executing_command("");
        }

        [Test]
        public void file_is_imported()
        {
            var packageInfo = Environment.CurrentDirectoryRepository.PackagesByName["package"].First();
            var export = packageInfo.Load().Content.FirstOrDefault(x=>x.Key == "bin-net35")
                    .ShouldNotBeNull();
            export.Select(x=>x.File)
                .ShouldHaveCountOf(1)
                .First()
                    .Name.ShouldBe(System.IO.Path.GetFileName(_filePath));
        }
    }
}