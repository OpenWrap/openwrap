using System.Collections.Generic;
using System.Linq;
using OpenWrap.PackageModel;

namespace OpenWrap.PackageManagement.DependencyResolvers
{
    public class DependencyCombinator
    {
        // this is a very naive implementation. Something better could be written, feel free.
        readonly List<VersionVertex> _versionVertices;

        public DependencyCombinator(IEnumerable<VersionVertex> versionVertices)
        {
            _versionVertices = versionVertices.ToList();
        }

        public IEnumerable<VersionVertex> Combine(IEnumerable<VersionVertex> newVersions)
        {
            newVersions = newVersions.ToList();
            var softMin = AllOfType<GreaterThanOrEqualVersionVertex>(newVersions)
                .Select(_ => _.Version)
                .Concat(AllOfType<PessimisticGreaterVersionVertex>(newVersions)
                            .Select(_ => _.Version))
                .OrderByDescending(_ => _)
                .FirstOrDefault();

            var softMax = AllOfType<LessThanOrEqualVersionVertex>(newVersions)
                .Select(_ => _.Version)
                .OrderBy(_ => _)
                .FirstOrDefault();

            var hardMin = AllOfType<GreaterThanVersionVertex>(newVersions)
                .Select(_ => _.Version)
                .OrderByDescending(_ => _)
                .FirstOrDefault();

            var hardMax = AllOfType<LessThanVersionVertex>(newVersions)
                .Select(_ => _.Version)
                .Concat(AllOfType<PessimisticGreaterVersionVertex>(newVersions).Select(_ => _.MaxVersion))
                .OrderBy(_ => _)
                .FirstOrDefault();

            var min = softMin ?? hardMin;
            var max = softMax ?? hardMax;

            if (min != null && max != null && (max < min || min > max)) yield break;

            var equal = AllOfType<EqualVersionVertex>(newVersions)
                .Select(_ => _.Version)
                .Distinct();

            var hardEqual = AllOfType<AbsolutelyEqualVersionVertex>(newVersions)
                .Select(_ => _.Version)
                .Distinct();
            var firstHardEqual = hardEqual.FirstOrDefault();

            SemanticVersion combinedEqual;
            if (!EqualityCompatible(equal.Concat(hardEqual).ToList(), out combinedEqual))
                yield break;

            if (combinedEqual != null)
            {

                if ((softMax == null || combinedEqual <= softMax) &&
                    (hardMax == null || combinedEqual < hardMax) &&
                    (softMin == null || combinedEqual >= softMin) &&
                    (hardMin == null || combinedEqual > hardMin))
                    yield return firstHardEqual != null
                        ? (VersionVertex)new AbsolutelyEqualVersionVertex(firstHardEqual)
                        : new EqualVersionVertex(combinedEqual);
                yield break;
            }

            if (softMin != null && hardMin != null)
                yield return softMin > hardMin
                                 ? (VersionVertex)new GreaterThanOrEqualVersionVertex(softMin)
                                 : new GreaterThanVersionVertex(hardMin);
            else if (softMin != null) yield return new GreaterThanOrEqualVersionVertex(softMin);
            else if (hardMin != null) yield return new GreaterThanVersionVertex(hardMin);

            if (softMax != null && hardMax != null)
                yield return softMax < hardMax
                                 ? (VersionVertex)new LessThanOrEqualVersionVertex(softMax)
                                 : new LessThanVersionVertex(hardMax);
            else if (softMax != null) yield return new LessThanOrEqualVersionVertex(softMax);
            else if (hardMax != null) yield return new LessThanVersionVertex(hardMax);
        }

        IEnumerable<T> AllOfType<T>(IEnumerable<VersionVertex> newVersions)
        {
            return newVersions.OfType<T>()
                .Concat(_versionVertices.OfType<T>());
        }

        bool EqualityCompatible(IEnumerable<SemanticVersion> equality, out SemanticVersion finalVersion)
        {
            finalVersion = null;

            var majorMatch = equality.Select(_ => _.Major).Distinct().ToList();
            var minorMatch = equality.Select(_ => _.Minor).Where(_ => _ != -1).DefaultIfEmpty(-1).Distinct().ToList();
            var patchMatch = equality.Select(_ => _.Patch).Where(_ => _ != -1).DefaultIfEmpty(-1).Distinct().ToList();
            var builds = equality.Select(_ => _.Build).Where(_ => _ != null).DefaultIfEmpty(null).Distinct().ToList();

            if (majorMatch.Count > 1 || minorMatch.Count > 1 || patchMatch.Count > 1 || builds.Count > 1)
                return false;
            if (majorMatch.Count == 0 || minorMatch.Count == 0 || patchMatch.Count == 0 || builds.Count == 0)
                return true;
            finalVersion = new SemanticVersion(majorMatch.FirstOrDefault(), minorMatch.FirstOrDefault(), patchMatch.FirstOrDefault(), build: builds.FirstOrDefault());
            return true;
        }
    }
}