using System;
using System.Diagnostics;
using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using OpenWrap.Commands;
using OpenWrap.Services;
using OpenWrap.VisualStudio.Interop;

namespace OpenWrap.VisualStudio.Hooks
{
    public class UICommandsPlugin : IDisposable
    {
        readonly ICommandRepository _commands;
        uint _commandTargetCookie;
        VsCommandManager _manager;

        public UICommandsPlugin() : this(ServiceLocator.GetService<ICommandRepository>())
        {
        }

        public UICommandsPlugin(ICommandRepository commands)
        {
            _commands = commands;
        }

        ~UICommandsPlugin()
        {
            Dispose(false);
        }

        public static DTE DTE { get { return SiteManager.GetGlobalService<DTE>(); } }
        public static IVsProfferCommands3 VsProfferCommands { get { return GetService<SVsProfferCommands, IVsProfferCommands3>(); } }

        static T1 GetService<T, T1>() where T1 : class
        {
            var ole = DTE as IOleServiceProvider;
            object instance;
            var reqId = typeof(T).GUID;
            ole.QueryService(ref reqId, ref reqId, out instance);
            return instance as T1;
        }

        public static IVsRegisterPriorityCommandTarget VsRegisterPriorityCommandTarget { get { return GetService<SVsRegisterPriorityCommandTarget, IVsRegisterPriorityCommandTarget>(); }}
        static readonly object _lock = new object();
        static bool _initialized;

        public void Initialize()
        {
            lock (_lock)
            {
                if (_initialized) return;
                //System.Diagnostics.Debugger.Launch();
                _initialized = true;

                _manager = new VsCommandManager();
                if (VsRegisterPriorityCommandTarget.RegisterPriorityCommandTarget(0u, _manager, out _commandTargetCookie) < 0)
                    return;
                foreach (var command in _commands.OfType<IUICommandDescriptor>())
                    if (!_manager.TryAdd(command))
                        Debug.WriteLine(string.Format("Could not add command '{1}-{0}'.", command.Noun, command.Verb));
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            try
            {
                var target = VsRegisterPriorityCommandTarget;
                if (target != null)
                    VsRegisterPriorityCommandTarget.UnregisterPriorityCommandTarget(_commandTargetCookie);
                _manager.Dispose();
            }
            catch{}
        }
    }
}