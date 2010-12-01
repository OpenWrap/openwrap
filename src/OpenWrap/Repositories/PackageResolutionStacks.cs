using System.Collections.Generic;
using System.Linq;

namespace OpenWrap.Repositories
{
    public class PackageResolutionStacks
    {
        static PackageResolutionStacks()
        {
            Null = new PackageResolutionStacks();
        }
        public static PackageResolutionStacks Null { get; private set; }
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

        public PackageResolutionStacks Combine(IEnumerable<CallStack> successful, IEnumerable<CallStack> failed)
        {
            return new PackageResolutionStacks(Successful.Concat(successful), Failed.Concat(failed));
        }
        public IEnumerable<CallStack> Successful { get; private set; }
        public IEnumerable<CallStack> Failed { get; private set; }
    }
}