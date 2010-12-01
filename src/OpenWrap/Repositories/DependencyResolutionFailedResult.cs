using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Commands;
using OpenWrap.PackageManagement;

namespace OpenWrap.Repositories
{
    public class PackageConflictsOutput : Error
    {
        public PackageConflictResult Result { get; set; }
        
        static readonly string MESSAGE = "Dependency {0} is in conflict [{1}]." + Environment.NewLine + "{2}";

        public PackageConflictsOutput(PackageConflictResult result)
        {
            Result = result;
        }

        public override string ToString()
        {
            return string.Format(MESSAGE,
                                 Result.Package.Identifier.Name,
                                 Result.Package.Packages.Select(x => x.Version.ToString()).Join(", "),
                                 Result.Package.DependencyStacks.Select(x => "\t" + x.ToString()).Join(Environment.NewLine));
        }
    }
    public class PackageMissingOutput : Error {

        public PackageMissingResult Result { get; private set; }
        static readonly string MESSAGE = "Dependency {0} wasn't found.";
        public PackageMissingOutput(PackageMissingResult result)
        {
            Result = result;
            
        }
        public override string  ToString()
        {
            return string.Format(MESSAGE, Result.Package.Identifier.Name);
        }
    }
}