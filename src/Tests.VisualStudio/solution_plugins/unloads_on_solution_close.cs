using NUnit.Framework;
using OpenWrap.Commands.Core;
using OpenWrap.Testing;
using Tests.VisualStudio.contexts;

namespace Tests.VisualStudio.solution_plugins
{
    public class unloads_on_solution_close : visual_studio
    {
        public unloads_on_solution_close()
        {
            given_solution_addin_com_reg();
            given_solution_file("mySolution.sln", true);
            given_project_2010("MyProject");
            given_file("Class1.cs", "public class ClassName { public static void MainMethod() {} }");

            given_command("init-wrap . -name MyProject -all");
            given_vs_action(
                dte => dte.Solution.SaveAll(true),
                dte => dte.Windows.Output("OpenWrap").WaitForMessage(StartSolutionPlugin.SOLUTION_PLUGINS_STARTED),
                dte => dte.Solution.Close(CloseOptions.Wait));

            when_executing_vs2010();
        }

        [Test]
        public void solution_plugins_loaded()
        {
            Output.ShouldContain(StartSolutionPlugin.SOLUTION_PLUGINS_STARTED);
        }

        [Test]
        public void solution_plugins_unloaded()
        {
            Output.ShouldContain(StartSolutionPlugin.SOLUTION_PLUGIN_UNLOADING);
        }
    }
}