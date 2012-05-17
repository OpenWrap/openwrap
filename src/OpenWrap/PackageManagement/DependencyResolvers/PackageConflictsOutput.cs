using System.Linq;
using OpenWrap.Commands;

namespace OpenWrap.PackageManagement.DependencyResolvers
{
    public class PackageConflictsOutput : Error
    {
        static readonly string MESSAGE = "Dependency {0} is in conflict [{1}]." + System.Environment.NewLine + "{2}";

        public PackageConflictsOutput(PackageConflictResult result)
        {
            Result = result;
        }

        public PackageConflictResult Result { get; set; }

        public override string ToString()
        {
            return string.Format(MESSAGE,
                                 Result.Package.Identifier.Name,
                                 Result.Package.Packages.Select(x => x.SemanticVersion.ToString()).JoinString(", "),
                                 Result.Package.DependencyStacks.Select(x => "\t" + x.ToString()).JoinString(System.Environment.NewLine));
        }
    }
}