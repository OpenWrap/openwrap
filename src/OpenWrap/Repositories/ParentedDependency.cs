using System.Collections.Generic;
using System.Linq;
using OpenWrap.Dependencies;

namespace OpenWrap.Repositories
{
    public class ParentedDependency
    {
        public ParentedDependency(PackageDependency dependency, ParentedDependency parent)
        {
            Dependency = dependency;
            Parent = parent;
        }

        public PackageDependency Dependency { get; private set; }
        public ParentedDependency Parent { get; private set; }
        public override string ToString()
        {
            var list = new List<ParentedDependency>();
            var current = this;
            while (current != null)
            {
                list.Add(current);
                current = current.Parent;
            }
            list.Reverse();
            return list.Select(x => x.Dependency.ToString()).Join(" -> ");
        }
    }
}