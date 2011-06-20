extern alias resharper450;
extern alias resharper500;
extern alias resharper510;
using System;
using NUnit.Framework;
using OpenWrap.Testing;
using OpenWrap.VisualStudio.SolutionPlugins.ReSharper;
using Tests.VisualStudio.contexts;

namespace Tests.VisualStudio.resharper
{
    [Category("ReSharper")]
    public class resharper_plugin_manager_loads : visual_studio
    {
        public resharper_plugin_manager_loads()
        {
            given_openwrap_assemblyOf<ReSharperLoaderPlugin>("solution");
            given_openwrap_assemblyOf<resharper450::OpenWrap.Resharper.PluginManager>("solution");
            given_openwrap_assemblyOf<resharper500::OpenWrap.Resharper.PluginManager>("solution");
            given_openwrap_assemblyOf<resharper510::OpenWrap.Resharper.PluginManager>("solution");

            given_solution_addin_com_reg();
            given_solution_file("mySolution.sln", true);
            given_project_2010("MyProject");
            given_file("Class1.cs", "public class ClassName { public static void MainMethod() {} }");

            given_command("init-wrap . -name MyProject -all");

            given_vs_action(dte => dte.Solution.SaveAll(true));

            when_loading_solution_with_plugins();
        }

        [Test]
        public void resharper_loader_loaded()
        {

            Output.ShouldContain("Resharper Plugin Manager loaded");
            Console.Write(Output);
        }
    }
}