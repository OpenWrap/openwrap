using System.Collections.Generic;
using System.Linq;

namespace OpenWrap.PackageManagement.DependencyResolvers
{
    public class PackageResolutionStacks
    {
        static PackageResolutionStacks()
        {
            Null = new PackageResolutionStacks();
        }

        public PackageResolutionStacks()
        {
            Successful = Enumerable.Empty<CallStack>();
            Failed = Enumerable.Empty<CallStack>();
        }

        public PackageResolutionStacks(IEnumerable<CallStack> successful, IEnumerable<CallStack> failed)
        {
            Successful = successful.ToList().AsReadOnly();
            Failed = failed.ToList().AsReadOnly();
        }

        public static PackageResolutionStacks Null { get; private set; }

        public IEnumerable<CallStack> Failed { get; private set; }
        public IEnumerable<CallStack> Successful { get; private set; }

        public PackageResolutionStacks Combine(IEnumerable<CallStack> successful, IEnumerable<CallStack> failed)
        {
            return new PackageResolutionStacks(Successful.Concat(successful), Failed.Concat(failed));
        }
    }
}