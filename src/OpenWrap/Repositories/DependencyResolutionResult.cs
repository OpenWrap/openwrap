using System;
using System.Collections.Generic;
using System.Reflection;
using OpenWrap.Commands;

namespace OpenWrap.Repositories
{
    public class DependencyResolutionResult
    {
        public IEnumerable<ResolvedDependency> Dependencies { get; set; }

        public bool IsSuccess { get; set; }
    }
}
