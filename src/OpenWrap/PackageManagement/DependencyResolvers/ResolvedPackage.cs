using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.PackageModel;

namespace OpenWrap.PackageManagement.DependencyResolvers
{
    public class ResolvedPackage
    {
        public ResolvedPackage(PackageIdentifier packageIdentifier, IEnumerable<IPackageInfo> packages, IEnumerable<CallStack> dependencies)
        {
            Identifier = packageIdentifier;
            Packages = packages.ToList().AsReadOnly();
            DependencyStacks = dependencies.ToList().AsReadOnly();
        }

        public IEnumerable<CallStack> DependencyStacks { get; private set; }
        public PackageIdentifier Identifier { get; private set; }

        public bool IsAnchored
        {
            get
            {
                return DependencyStacks.Any(stack => stack.OfType<DependencyNode>().Where(x => x.Dependency.Name.EqualsNoCase(Identifier.Name)).Any(x => x.Dependency.Anchored))
                       || Packages.Any(x => x.Anchored);
            }
        }

        public IEnumerable<IPackageInfo> Packages { get; private set; }
    }
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