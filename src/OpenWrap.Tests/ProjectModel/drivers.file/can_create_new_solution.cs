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
}