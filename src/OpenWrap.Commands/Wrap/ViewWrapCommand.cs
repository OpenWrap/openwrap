using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.PackageManagement;
using OpenWrap.PackageModel;

namespace OpenWrap.Commands.Wrap
{
    [Command(Noun = "wrap", Verb = "view")]
    public class ViewWrapCommand : WrapCommand
    {
        bool? _project;

        [CommandInput(IsRequired = true, Position = 0)]
        public string Name { get; set; }

        [CommandInput]
        public Version Version { get; set; }

        [CommandInput]
        public bool System { get; set; }

        [CommandInput]
        public bool Project
        {
            get { return _project ?? !System; }
            set { _project = value; }
        }

        public override IEnumerable<ICommandOutput> Execute()
        {
            return Either(VerifyInputs()).Or(ExecuteCore());
        }

        IEnumerable<ICommandOutput> ExecuteCore()
        {
            IPackageInfo item = null;

            if ( System )
            {
                item = PackageInfo(Environment.SystemRepository.PackagesByName[Name]);
            }
            if (Project)
            {
                item = PackageInfo(Environment.ProjectRepository.PackagesByName[Name]);
            }

            if (item == null)
                yield return new Error("No package found named {1}", Name);
            else
                yield return new ViewWrapCommandOutput(item);
        }

        IEnumerable<ICommandOutput> VerifyInputs()
        {
            if (System && !Environment.SystemRepository.PackagesByName[Name].Any())
                yield return new Error("Cannot find package named '{0}' in system repository.", Name);
            if (Project && Environment.ProjectRepository == null)
                yield return new Error("Not in a package directory.");
            if (Project && Environment.ProjectRepository != null && !Environment.ProjectRepository.PackagesByName[Name].Any())
                yield return new Error("Cannot find package named '{0}' in project repository.", Name);
        }

        IPackageInfo PackageInfo(IEnumerable<IPackageInfo> packageInfos)
        {
            if (Version != null)
                return packageInfos.OrderByDescending(x => x.Version).FirstOrDefault(x => x.Version.Major.Equals(Version.Major) && x.Version.Major.Equals(Version.Major));

            return packageInfos.Last();
        }
    }
}