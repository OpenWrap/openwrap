using NUnit.Framework;
using OpenWrap.IO;
using OpenWrap.Testing;

namespace Tests.ProjectModel.drivers.file
{
    public class can_roundtrip_solution : visual_studio
    {
        static string[] Solutions = new[]
        {
            Files.dotless,
            Files.fluentnhibernate,
            Files.openrasta,
            Files.openwrap,
            Files.ironjs,
            Files.monocecil,
            Files.restsharp,
            Files.gitdotaspx
        };
        [Test, TestCaseSource("Solutions")]
        public void solution_roundtrips(string solutionFile)
        {
            if (!solutionFile.EndsWith("\r\n"))
                solutionFile += "\r\n";
            given_solution_file("solution.sln", solutionFile);
            when_reading_solution();
            Solution.Save();
            SlnFile.ReadString().ShouldBe(solutionFile);

        }
    }
}