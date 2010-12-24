using System.Collections.Generic;
using System.Linq;
using OpenWrap.Collections;

namespace OpenWrap.PackageManagement.DependencyResolvers
{
    public class DependencyResolutionResult
    {
        public DependencyResolutionResult(IEnumerable<ResolvedPackage> successfulPackages, IEnumerable<ResolvedPackage> conflictingPackages, IEnumerable<ResolvedPackage> missingPackages)
        {
            SuccessfulPackages = successfulPackages.ToList().AsReadOnly();
            DiscardedPackages = conflictingPackages.ToList().AsReadOnly();
            MissingPackages = missingPackages.ToList().AsReadOnly();
            //IsSuccess = !(MissingPackages.Any() || DiscardedPackages.Any());
            IsSuccess = !(MissingPackages.Any(x => SuccessfulPackages.None(s => s.Identifier.Name.EqualsNoCase(x.Identifier.Name)))
                          || DiscardedPackages.Any(x => SuccessfulPackages.None(s => s.Identifier.Name.EqualsNoCase(x.Identifier.Name))));
        }

        public IEnumerable<ResolvedPackage> DiscardedPackages { get; private set; }

        public bool IsSuccess { get; private set; }
        public IEnumerable<ResolvedPackage> MissingPackages { get; private set; }
        public IEnumerable<ResolvedPackage> SuccessfulPackages { get; private set; }
    }
}