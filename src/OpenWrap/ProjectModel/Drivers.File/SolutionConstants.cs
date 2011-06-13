using System;
using System.Collections.Generic;

namespace OpenWrap.ProjectModel.Drivers.File
{
    public static class SolutionConstants
    {

        static SolutionConstants()
        {
            Editions =new Dictionary<Version, string>
            {
                { VisualStudio2008Version, "Visual Studio 2008" },
                { VisualStudio2010Version, "Visual Studio 2010" }
            };
        }
        public static readonly IDictionary<Version, string> Editions;

        public static readonly Version VisualStudio2008Version = new Version(10, 0);
        public static readonly Version VisualStudio2010Version = new Version(11, 0);
    }
}