using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.Services;

namespace OpenWrap.Commands.Wrap
{
    [Command(Noun = "wrap", Verb = "clean")]
    public class CleanWrapCommand : WrapCommand
    {
        [CommandInput(Position = 0)]
        public string Name { get; set; }

        [CommandInput]
        public bool System { get; set; }

        bool? _project;

        [CommandInput]
        public bool Project
        {
            get { return _project ?? false; }
            set { _project = value; }
        }

        public override IEnumerable<ICommandOutput> Execute()
        {
            return Either(VerifyInputs()).Or(ExecuteCore());
        }

        IEnumerable<ICommandOutput> ExecuteCore()
        {
            IEnumerable<ICommandOutput> outputs = Enumerable.Empty<ICommandOutput>();
            if (IncludeProject)
                outputs = outputs.Concat((string.IsNullOrEmpty(Name)
                                                ? PackageManager.CleanProjectPackages(Environment.Descriptor, Environment.ProjectRepository)
                                                : PackageManager.CleanProjectPackages(Environment.Descriptor, Environment.ProjectRepository, Name))
                                          .Select(ToOutput));
            if (System)
                outputs = outputs.Concat((string.IsNullOrEmpty(Name)
                                                ? PackageManager.CleanSystemPackages(Environment.SystemRepository)
                                                : PackageManager.CleanSystemPackages(Environment.SystemRepository, Name))
                                          .Select(ToOutput));

            return outputs;
        }
        bool IncludeProject
        {
            get { return (_project == null && System == false) || (_project != null && _project.Value); }
        }
        IEnumerable<ICommandOutput> VerifyInputs()
        {
            // verify presence of project if IncludeProject
            // verify presence of system repo
            yield break;

        }
    }
}
