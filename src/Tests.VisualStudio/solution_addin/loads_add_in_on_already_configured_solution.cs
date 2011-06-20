using System.Linq;
using EnvDTE;
using NUnit.Framework;
using OpenWrap.Commands.Core;
using OpenWrap.Testing;
using OpenWrap.VisualStudio.SolutionAddIn;
using Tests.VisualStudio.contexts;

namespace Tests.VisualStudio.solution_addin
{
    public class loads_add_in_on_already_configured_solution : visual_studio
    {
        public loads_add_in_on_already_configured_solution()
        {
            given_solution_addin_com_reg();
            given_solution_file("mySolution.sln", true);
            given_project_2010("MyProject");
            given_file("Class1.cs", "public class ClassName { public static void MainMethod() {} }");

            given_command("init-wrap . -name MyProject -all");
            given_vs_action(dte => dte.Solution.SaveAll(true));

            when_loading_solution_with_plugins();
        }

        [Test]
        public void solution_addin_is_loaded()
        {
            Dte.Solution.AddIns.Update();
            Dte.Solution.AddIns.OfType<AddIn>().Where(x => x.ProgID == ComConstants.ADD_IN_PROGID_2010).ShouldHaveCountOf(1);
        }

        [Test]
        public void start_solutionplugin_command_loaded()
        {
            Output.ShouldContain(StartSolutionPlugin.SOLUTION_PLUGIN_STARTING);
        }
    }
}