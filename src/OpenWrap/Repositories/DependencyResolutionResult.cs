using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using OpenWrap.Commands;

namespace OpenWrap.Repositories
{
    public class DependencyResolutionResult
    {
        public DependencyResolutionResult(IEnumerable<ResolvedPackage> successfulPackages, IEnumerable<ResolvedPackage> conflictingPackages, IEnumerable<ResolvedPackage> missingPackages)
        {
            SuccessfulPackages = successfulPackages.ToList().AsReadOnly();
            ConflictingPackages = conflictingPackages.ToList().AsReadOnly();
            MissingPackages = missingPackages.ToList().AsReadOnly();
            IsSuccess = !(MissingPackages.Any() || ConflictingPackages.Any());
        }

        public IEnumerable<ResolvedPackage> ConflictingPackages { get; private set; }

        public IEnumerable<ResolvedPackage> SuccessfulPackages { get; private set; }

        public IEnumerable<ResolvedPackage> MissingPackages { get; private set; }

        public bool IsSuccess { get; private set; }
    }
}
