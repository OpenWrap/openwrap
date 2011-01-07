using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenFileSystem.IO;
using OpenWrap.PackageManagement;
using OpenWrap.Runtime;
using OpenWrap.Testing;


namespace OpenWrap.Commands.Wrap
{
    [Command(Noun="wrap", Verb="test")]
    public class TestWrapCommand : AbstractCommand
    {
        readonly IFileSystem _fileSystem;
        readonly IPackageResolver _resolver;
        IEnvironment _environment;
        IPackageExporter _exporter;

        public TestWrapCommand()
            : this(Services.Services.GetService<IFileSystem>(),
            Services.Services.GetService<IPackageResolver>(),
            Services.Services.GetService<IEnvironment>(),
            Services.Services.GetService<IPackageExporter>())
        {
        }
        public TestWrapCommand(IFileSystem fileSystem,IPackageResolver resolver, IEnvironment environment, IPackageExporter exporter)
        {
            _fileSystem = fileSystem;
            _exporter = exporter;
            _resolver = resolver;
            _environment = environment;
        }

        [CommandInput(Position=0, IsRequired=true)]
        public string Name { get; set; }
        public override IEnumerable<ICommandOutput> Execute()
        {
            var package = _environment.CurrentDirectoryRepository.PackagesByName[Name]
                .OrderByDescending(x=>x.Version)
                .FirstOrDefault();

            if (package == null)
            {
                yield return new Error("Package not found.");
                yield break;
            }
            var testRunner = new TestRunnerManager(_fileSystem, _environment, _resolver, _exporter);
            foreach (var result in testRunner.ExecuteAllTests(_environment.ExecutionEnvironment, package.Load()))
            {
                if (result.Value == true)
                    yield return new GenericMessage(result.Key);
                else if (result.Value == false)
                    yield return new Error(result.Key);
                else
                    yield return new Warning(result.Key);
            }
        }
    }
}
