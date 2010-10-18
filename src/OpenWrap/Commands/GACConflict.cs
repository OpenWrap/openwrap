using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.Repositories;

namespace OpenWrap.Commands
{
    public class GACConflict : Warning
    {
        public ResolvedDependency Dependency { get; protected set; }
        const string MESSAGE = "The dependency {0} exists in the GAC";

        public GACConflict(ResolvedDependency dependency) : base(string.Format(MESSAGE,dependency.Dependency))
        {
            Dependency = dependency;
        }
    }
}
