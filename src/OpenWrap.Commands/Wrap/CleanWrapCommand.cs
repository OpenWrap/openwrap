using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;
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
        protected override IEnumerable<System.Func<IEnumerable<ICommandOutput>>> Validators()
        {
            yield return VerifyInputs;
        }

        protected override IEnumerable<ICommandOutput> ExecuteCore()
        {
            IEnumerable<ICommandOutput> outputs = Enumerable.Empty<ICommandOutput>();
            if (IncludeProject)
            {
                
                var descriptors = HostEnvironment.ScopedDescriptors.Select(x=>x.Value.Value.Lock(HostEnvironment.ProjectRepository));

                outputs = outputs.Concat((string.IsNullOrEmpty(Name)
                                              ? PackageManager.CleanProjectPackages(descriptors, HostEnvironment.ProjectRepository)
                                              : PackageManager.CleanProjectPackages(descriptors, HostEnvironment.ProjectRepository, Name))
                                             .Select(ToOutput));
            }
            if (System)
                outputs = outputs.Concat((string.IsNullOrEmpty(Name)
                                                ? PackageManager.CleanSystemPackages(HostEnvironment.SystemRepository)
                                                : PackageManager.CleanSystemPackages(HostEnvironment.SystemRepository, Name))
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
