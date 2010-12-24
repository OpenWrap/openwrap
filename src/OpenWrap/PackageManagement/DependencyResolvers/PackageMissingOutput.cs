using OpenWrap.Commands;

namespace OpenWrap.PackageManagement.DependencyResolvers
{
    public class PackageMissingOutput : Error
    {
        static readonly string MESSAGE = "Dependency {0} wasn't found.";

        public PackageMissingOutput(PackageMissingResult result)
        {
            Result = result;
        }

        public PackageMissingResult Result { get; private set; }

        public override string ToString()
        {
            return string.Format(MESSAGE, Result.Package.Identifier.Name);
        }
    }
}