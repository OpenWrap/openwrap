using System.IO;
using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands.contexts;
using OpenWrap.Commands.Wrap;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;
using OpenWrap.Runtime;
using OpenWrap.Testing;

namespace OpenWrap.Tests.Commands.Wrap
{
    [TestFixture("test.txt", "files; file=bin-net35->test.txt")]
    [TestFixture("test.txt", "files; file = bin-net35 -> test.txt ")]
    public class from_existing_files : command_context<BuildWrapCommand>
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

            when_executing_command();
        }

        [Test]
        public void file_is_imported()
        {
            var packageInfo = Environment.CurrentDirectoryRepository.PackagesByName["package"].First();
            var export = packageInfo.Load().GetExport("bin-net35", new ExecutionEnvironment())
                    .ShouldNotBeNull();
            var exports= export.Items;
            exports.ShouldHaveCountOf(1)
                    .Select(x => FileSystem.GetFile(x.FullPath)).First()
                    .Name.ShouldBe(System.IO.Path.GetFileName(_filePath));
        }
    }
}