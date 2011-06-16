using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using OpenWrap.Preloading;

namespace OpenWrap.VisualStudio.SolutionAddIn
{
    public class AddInAppDomainManager : MarshalByRefObject
    {
        
        public override object InitializeLifetimeService()
        {
            return null;
        }

        public AddInAppDomainManager()
        {
            var appDomain = AppDomain.CurrentDomain;
            ThreadPool.QueueUserWorkItem(state => Load((string)appDomain.GetData("openwrap.vs.version"),
                                                       (string)appDomain.GetData("openwrap.vs.currentdirectory"),
                                                       (string[])appDomain.GetData("openwrap.vs.packages"),
                                                       (AppDomain)appDomain.GetData("openwrap.appdomain")));
            AppDomain.CurrentDomain.AssemblyResolve += (src, ev) =>
            {
                var currentAssembly = typeof(AddInAppDomainManager).Assembly;
                return (ev.Name == currentAssembly.GetName().Name) ? currentAssembly : null;
            };
        }

        void Load(string vsVersion, string currentDirectory, string[] packagePaths, AppDomain sourceAppDomain)
        {
            var assemblies = Preloader.LoadAssemblies(packagePaths);
            Func<IDictionary<string, object>, int> runner = null;
            foreach(var asm in assemblies)
            {
                try
                {
                    var runnerType = (from type in asm.Key.GetExportedTypes()
                                      where type.Name.EndsWith("Runner")
                                      let mi = type.GetMethod("Main", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(IDictionary<string, object>) }, null)
                                      where mi != null
                                      select mi).FirstOrDefault();
                    if (runnerType != null)
                    {
                        runner = env => (int)runnerType.Invoke(null, new object[] { env });
                        break;
                    }
                }
                catch
                {
                }
            }
            if (runner == null) return;
            var info = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                { "openwrap.cd", currentDirectory },
                { "openwrap.shell.commandline", "start-solutionplugin" },
                { "openwrap.shell.assemblies", assemblies.ToList() },
                { "openwrap.shell.version", "1.1" },
                { "openwrap.shell.type", "VisualStudio." + vsVersion },
                { "openwrap.scope", "build"},
                { "openwrap.shell.appdomain", sourceAppDomain },
                { "openwrap.shell.formatter", "OpenWrap.VisualStudio.Shared.OutputWindowCommandOutputFormatter, OpenWrap.VisualStudio.Shared" }
            };
            
            runner(info);

        }
    }
}