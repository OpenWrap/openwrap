using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.Repositories;
using OpenWrap.Services;

namespace OpenWrap.Commands.Wrap
{
    [Command(Noun="wrap", Verb="list")]
    public class ListWrapCommand : AbstractCommand
    {
        [CommandInput]
        public bool System { get; set; }

        protected IEnvironment Environment { get { return WrapServices.GetService<IEnvironment>(); } }
        public override IEnumerable<ICommandOutput> Execute()
        {
            var repoToList = (System ? Environment.SystemRepository : Environment.ProjectRepository);

            return repoToList.PackagesByName.NotNull()
                    .Select(x => (ICommandOutput)new PackageDescriptionOutput(x.Key, x));
        }
    }

    public class PackageDescriptionOutput : GenericMessage
    {
        public PackageDescriptionOutput(string packageName, IEnumerable<IPackageInfo> packageVersions)
            : base(" - {0}\r\n   Versions: {1}", packageName, string.Join(", ", packageVersions.Select(x => x.Version.ToString()).ToArray()))
        {
        }
    }
}
