using System;
using NUnit.Framework;
using OpenWrap.IO;
using OpenWrap.ProjectModel.Drivers.File;
using OpenWrap.Testing;

namespace Tests.ProjectModel.drivers.file
{
    [TestFixture("11.00", "2010")]
    [TestFixture("10.00", "2008")]
    public class can_create_new_solution : visual_studio
    {
        readonly string ExpectedVersion;
        readonly string ExpectedEdition;

        public can_create_new_solution(string targetVersion, string edition)
        {
            ExpectedVersion = targetVersion;
            ExpectedEdition = edition;
            given_solution("solution.sln", targetVersion == "11.00" ? SolutionConstants.VisualStudio2010Version : SolutionConstants.VisualStudio2008Version);
            when_writing_solution();
        }

        [Test]
        public void empty_solution_is_created()
        {
            SlnContent.ShouldBe("\r\nMicrosoft Visual Studio Solution File, Format Version " + ExpectedVersion + "\r\n" +
                                "# Visual Studio " + ExpectedEdition + "\r\n");
        }
    }

    [TestFixture("11.00", "2010", SolutionConstants.ADD_IN_PROGID_2010)]
    [TestFixture("10.00", "2008", SolutionConstants.ADD_IN_PROGID_2008)]
    public class can_create_new_solution_with_addin : visual_studio
    {
        readonly string ExpectedVersion;
        readonly string ExpectedEdition;
        readonly string ExpectedProgId;

        public can_create_new_solution_with_addin(string targetVersion, string edition, string progid)
        {
            ExpectedVersion = targetVersion;
            ExpectedEdition = edition;
            ExpectedProgId = progid;
            given_solution("solution.sln", 
                targetVersion == "11.00" ? SolutionConstants.VisualStudio2010Version : SolutionConstants.VisualStudio2008Version,
                true);
            when_writing_solution();
        }

        [Test]
        public void empty_solution_is_created()
        {
            SlnContent.ShouldBe("\r\nMicrosoft Visual Studio Solution File, Format Version " + ExpectedVersion + "\r\n" +
                                "# Visual Studio " + ExpectedEdition + "\r\n"+
                                "Global\r\n" +
                                    "\tGlobalSection(ExtensibilityAddIns) = postSolution\r\n" +
		                               "\t\t" + ExpectedProgId + " = 1;Ensures OpenWrap initialization when a solution is opened.;OpenWrap Visual Studio Solution Add-in\r\n" +
	                                "EndGlobalSection\r\n" +
                                "EndGlobal\r\n");
        }
    }
}