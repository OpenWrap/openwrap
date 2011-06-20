using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using OpenWrap.Preloading;

namespace OpenWrap.VisualStudio.SolutionAddIn
{
    public class AddInAppDomainManager : MarshalByRefObject, IDisposable
    {
        ManualResetEvent _manualResetEvent;

        public AddInAppDomainManager()
        {
            var appDomain = AppDomain.CurrentDomain;
            _manualResetEvent = new ManualResetEvent(false);
            appDomain.SetData("openwrap.vs.events.unloading", _manualResetEvent);
            AppDomain.CurrentDomain.AssemblyResolve += (src, ev) =>
            {
                var currentAssembly = typeof(AddInAppDomainManager).Assembly;
                return (new AssemblyName(ev.Name).Name == currentAssembly.GetName().Name) ? currentAssembly : null;
            };
            ThreadPool.QueueUserWorkItem(state => Load((string)appDomain.GetData("openwrap.vs.version"),
                                                       (string)appDomain.GetData("openwrap.vs.currentdirectory"),
                                                       (string[])appDomain.GetData("openwrap.vs.packages")));
        }

        // todo: make this return a lease so the object does't keep on living forever?
        public override object InitializeLifetimeService()
        {
            return null;
        }

        static void Load(string vsVersion, string currentDirectory, IEnumerable<string> packagePaths)
        {
            var assemblies = Preloader.LoadAssemblies(packagePaths);
            Func<IDictionary<string, object>, int> runner = null;
            foreach (var asm in assemblies)
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
                catch (Exception e)
                {
                    Debug.WriteLine(e);
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
                { "openwrap.scope", "build" },
                { "openwrap.shell.formatter", "OpenWrap.VisualStudio.OutputWindowCommandOutputFormatter, OpenWrap.VisualStudio.Shared" }
            };

            runner(info);
        }
        
        public void Dispose()
        {
            _manualResetEvent.Set();
            _manualResetEvent = null;
        }
    }
}