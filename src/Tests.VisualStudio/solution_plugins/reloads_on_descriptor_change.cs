using System;
using System.Text.RegularExpressions;
using NUnit.Framework;
using OpenWrap.Commands.Core;
using OpenWrap.PackageModel;
using OpenWrap.Testing;
using Tests.VisualStudio.contexts;

namespace Tests.VisualStudio.solution_addin
{
    public class reloads_on_descriptor_change : visual_studio
    {
        public reloads_on_descriptor_change()
        {
            given_solution_addin_com_reg();
            given_solution_file("mySolution.sln", true);
            given_project_2010("MyProject");
            given_file("Class1.cs", "public class ClassName { public static void MainMethod() {} }");

            given_command("init-wrap . -name MyProject -all");
            given_vs_action(
                dte => dte.Solution.SaveAll(true),
                dte => dte.Windows.Output("OpenWrap").WaitForMessage(StartSolutionPlugin.SOLUTION_PLUGINS_STARTED),
                dte => RootDir.GetFile("MyProject.wrapdesc").Touch(),
                dte => dte.Windows.Output("OpenWrap").WaitForMessage(StartSolutionPlugin.SOLUTION_PLUGINS_STARTED, 2));

            when_executing_vs2010();
        }

        [Test]
        public void app_domain_is_reloaded()
        {
            Regex.Matches(Output, StartSolutionPlugin.SOLUTION_PLUGINS_STARTED).Count.ShouldBe(2);
        }

        [Test]
        public void app_domain_is_unloaded()
        {
            Output.ShouldContain(StartSolutionPlugin.SOLUTION_PLUGIN_UNLOADING);
        }

        [Test]
        public void no_errors_are_raised()
        {
            Output.ShouldNotContain("Exception");
        }
    }
}