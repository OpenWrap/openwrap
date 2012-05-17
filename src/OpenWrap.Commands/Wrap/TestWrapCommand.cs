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
        IPackageManager _manager;

        public TestWrapCommand()
            : this(Services.ServiceLocator.GetService<IFileSystem>(),
            Services.ServiceLocator.GetService<IPackageResolver>(),
            Services.ServiceLocator.GetService<IEnvironment>(),
            Services.ServiceLocator.GetService<IPackageExporter>(),
            Services.ServiceLocator.GetService<IPackageManager>())
        {
        }
        public TestWrapCommand(IFileSystem fileSystem,IPackageResolver resolver, IEnvironment environment, IPackageExporter exporter, IPackageManager manager)
        {
            _fileSystem = fileSystem;
            _exporter = exporter;
            _manager = manager;
            _resolver = resolver;
            _environment = environment;
        }

        [CommandInput(Position=0, IsRequired=true)]
        public string Name { get; set; }

        protected override IEnumerable<ICommandOutput> ExecuteCore()
        {
            var package = _environment.CurrentDirectoryRepository.PackagesByName[Name]
                .OrderByDescending(x=>x.SemanticVersion)
                .FirstOrDefault();

            if (package == null)
            {
                yield return new Error("Package not found.");
                yield break;
            }
            var testRunner = new TestRunnerManager(new[]{new TdnetTestRunnerFactory(_fileSystem)}, _environment, _manager);
            yield return new Info("Starting testing package {0}...", package.Identifier);
            foreach (var result in testRunner.ExecuteAllTests(_environment.ExecutionEnvironment, package.Load()))
            {
                if (result.Value == true)
                    yield return new Info(result.Key);
                else if (result.Value == false)
                    yield return new Error(result.Key);
                else
                    yield return new Warning(result.Key);
            }
        }
    }
}
