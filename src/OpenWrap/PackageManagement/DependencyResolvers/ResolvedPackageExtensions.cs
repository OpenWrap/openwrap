using System.Collections.Generic;
using System.Linq;

namespace OpenWrap.PackageManagement.DependencyResolvers
{
    public static class ResolvedPackageExtensions
    {
        public static ILookup<string, IEnumerable<string>> GetFailures(this IEnumerable<PackageOperationResult> package)
        {
            var errors = (from missing in package.OfType<PackageMissingResult>()
                          from trace in missing.Package.DependencyStacks
                          select new
                          {
                                  who = trace.First(),
                                  traces = new[] { string.Format("not found: {0}", missing.Package.Identifier.Name) }
                                  .Concat(trace.Skip(1).Select(ParseStack))
                          })
                    .Union
                    (
                            from conflict in package.OfType<PackageConflictResult>()
                            from trace in conflict.Package.DependencyStacks
                            select new
                            {
                                    who = trace.First(),
                                    traces = new[] { string.Format("conflict: {0}", conflict.Package.Identifier.Name) }
                                    .Concat(trace.Skip(1).Select(ParseStack))
                            }
                    );
            return errors.ToLookup(x => x.who.ToString(), x => x.traces);
        }

        static string ParseStack(Node node)
        {
            return node is DependencyNode
                           ? "Dependends on " + node
                           : "Package version " + node;

        }
    }
}