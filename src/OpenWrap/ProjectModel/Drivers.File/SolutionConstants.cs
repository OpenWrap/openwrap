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

        public const string ADD_IN_PROGID_2010 = "OpenWrap.VisualStudio2010";
        public const string ADD_IN_PROGID_2008 = "OpenWrap.VisualStudio2008";
        public const string ADD_IN_NAME = "OpenWrap Visual Studio Solution Add-in";
        public const string ADD_IN_DESCRIPTION = "Ensures OpenWrap initialization when a solution is opened.";
    }
}