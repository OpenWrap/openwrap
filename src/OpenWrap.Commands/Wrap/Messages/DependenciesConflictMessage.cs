using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.PackageManagement.DependencyResolvers;
using OpenWrap.Repositories;

namespace OpenWrap.Commands.Wrap
{
    internal class DependenciesConflictMessage : Error
    {
        public List<IGrouping<string, ResolvedPackage>> ConflictingPackages { get; set; }

        public DependenciesConflictMessage(List<IGrouping<string, ResolvedPackage>> packageNames)
        {
            ConflictingPackages = packageNames;
            this.Type = CommandResultType.Error;
        }
        public override string ToString()
        {
            return "The following packages have conflicting dependencies:\r\n"
                   + ConflictingPackages.Select(x => 
                                                x.Key + " versions: " + x.Select(y=>y.Identifier.Version.ToString()).JoinString(", ")
                             ).JoinString(Environment.NewLine);
        }
    }
}