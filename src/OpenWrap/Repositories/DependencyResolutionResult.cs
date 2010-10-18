using System.Collections.Generic;
using OpenWrap.Commands;

namespace OpenWrap.Repositories
{
    public class DependencyResolutionResult
    {
        public DependencyResolutionResult()
        {
            Warnings = new Warning[] { };
        }

        public IEnumerable<ResolvedDependency> Dependencies { get; set; }
        public IEnumerable<Warning> Warnings { get; set; }
        public bool IsSuccess { get; set; }
    }
}
