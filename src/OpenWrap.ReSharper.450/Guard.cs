extern alias resharper;
using System;
using System.Threading;

#if v600
using ResharperPluginManager = resharper::JetBrains.Application.PluginSupport.PluginManager;
using ResharperPlugin = resharper::JetBrains.Application.PluginSupport.Plugin;
using ResharperPluginTitleAttribute = resharper::JetBrains.Application.PluginSupport.PluginTitleAttribute;
using ResharperPluginDescriptionAttribute = resharper::JetBrains.Application.PluginSupport.PluginDescriptionAttribute;
using ResharperSolutionComponentAttribute = resharper::JetBrains.ProjectModel.SolutionComponentAttribute;
using ResharperAssemblyReference = resharper::JetBrains.ProjectModel.IProjectToAssemblyReference;
using ResharperThreading = resharper::JetBrains.Threading.IThreading;
#else
using ResharperPluginManager = resharper::JetBrains.UI.Application.PluginSupport.PluginManager;
using ResharperPlugin = resharper::JetBrains.UI.Application.PluginSupport.Plugin;
using ResharperPluginTitleAttribute = resharper::JetBrains.UI.Application.PluginSupport.PluginTitleAttribute;
using ResharperPluginDescriptionAttribute = resharper::JetBrains.UI.Application.PluginSupport.PluginDescriptionAttribute;
using ResharperSolutionComponentAttribute = resharper::JetBrains.ProjectModel.SolutionComponentImplementationAttribute;
using ResharperThreading = OpenWrap.Resharper.IThreading;
using ResharperAssemblyReference = resharper::JetBrains.ProjectModel.IAssemblyReference;
#endif
namespace OpenWrap.Resharper
{
    public static class Guard
    {
        // ReSharper disable ConvertClosureToMethodGroup
        public static void Run(this ResharperThreading threading, string description, Action invoke)
        {
            threading.Dispatcher.Invoke(description,
                                        () => threading.ReentrancyGuard.Execute
                                                  (description, () => invoke()));
        }
        // ReSharper restore ConvertClosureToMethodGroup
        public static bool BeginInvokeAndWait<T>(this ResharperThreading threading, string description, Func<T> invoker, out T value, params WaitHandle[] waitHandles)
        {
            var guard = threading.ReentrancyGuard;
            var disp = guard.Dispatcher;
            T returnValue = default(T);
            ManualResetEvent finished = new ManualResetEvent(false);
            disp.BeginOrInvoke(description, () => guard.Execute(description, () => { returnValue = invoker();
                                                                                       finished.Set();}));
            var handles = new WaitHandle[waitHandles.Length + 1];
            waitHandles[0] = finished;
            waitHandles.CopyTo(handles, 1);

            var breakage = WaitHandle.WaitAny(waitHandles);
            value = returnValue;
            return breakage == 0;
        }
    }
}