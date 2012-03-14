using OpenWrap;
using OpenWrap.PackageManagement.DependencyResolvers;
using OpenWrap.PackageModel.Parsers;
using OpenWrap.Testing;

namespace Tests.Dependencies.versions.combinator
{
    public class context
    {
        protected string combining(string existingDependency, string newDependency)
        {
            var combined = combine(existingDependency, newDependency);
            var combinedReversed = combine(newDependency, existingDependency);
            combined.ShouldBe(combinedReversed);

            return combined.Length == 0 ? null : combined;
        }

        static string combine(string existingDependency, string newDependency)
        {
            return new DependencyCombinator(DependsParser.ParseVersions(existingDependency.Split(' ')))
                .Combine(DependsParser.ParseVersions(newDependency.Split(' ')))
                .JoinString(" and ");
        }
    }
}