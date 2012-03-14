using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenWrap.PackageManagement
{
    public class PackageMissingException : Exception
    {
        public IEnumerable<string> Paths { get; set; }

        public PackageMissingException(IEnumerable<string> packagePaths)
            : base(GenerateMessage(packagePaths))
        {
            Paths = packagePaths;
        }

        static string GenerateMessage(IEnumerable<string> packageNameAndPaths)
        {
            return "Cannot load initial packages. Try running the command with -UseSystem or deleting your wraps folder and doing an update-wrap."
                   + packageNameAndPaths.Aggregate(string.Empty, (@in, @out) => @in + Environment.NewLine + @out);
        }
    }
}