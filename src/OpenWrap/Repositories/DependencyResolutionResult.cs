using System.Collections.Generic;

namespace OpenWrap.Repositories
{
    public class DependencyResolutionResult
    {
        public IEnumerable<ResolvedDependency> Dependencies { get; set; }

        public bool IsSuccess { get; set; }
    }
}