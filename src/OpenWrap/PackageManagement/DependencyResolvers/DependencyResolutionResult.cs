using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Collections;
using OpenWrap.PackageModel;

namespace OpenWrap.PackageManagement.DependencyResolvers
{
    public class DependencyResolutionResult
    {
        public IPackageDescriptor Descriptor { get; private set; }

        public DependencyResolutionResult(IPackageDescriptor descriptor, IEnumerable<ResolvedPackage> successfulPackages, IEnumerable<ResolvedPackage> conflictingPackages, IEnumerable<ResolvedPackage> missingPackages)
        {
            Descriptor = descriptor;
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