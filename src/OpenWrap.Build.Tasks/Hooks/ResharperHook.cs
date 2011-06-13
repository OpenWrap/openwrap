extern alias resharper450;
extern alias resharper500;
extern alias resharper510;
extern alias resharper511;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using OpenWrap.Repositories;
using OpenFileSystem.IO;
using System.Diagnostics;
using OpenWrap.Runtime;


namespace OpenWrap.Build.Tasks
{
    static class ResharperLogger
    {
        public static void Debug(string text, params string[] args)
        {
            Debugger.Log(0, "resharper", DateTime.Now.ToShortTimeString() + ":" + string.Format(text, args) + "\r\n");
        }
    }

    public static class ResharperHook
    {
        const int WAIT_RETRY_MS = 5000;

        static Dictionary<Version, string> _integrationTypes = new Dictionary<Version, string>
        {
            {new Version("4.5.1288.2"), "OpenWrap.Resharper.ResharperIntegrationService, OpenWrap.Resharper.v450" },
            {new Version("5.0.1659.36"), "OpenWrap.Resharper.ResharperIntegrationService, OpenWrap.Resharper.v500" },
            {new Version("5.1.1727.12"), "OpenWrap.Resharper.ResharperIntegrationService, OpenWrap.Resharper.v510"}
        };

        static bool _called = false;
        static object _instance;
        static readonly object _lock = new object();
        static Timer _timer;
        static int _tries;
        static NotificationRegistration _queuedRegistration;

        public static void TryRegisterResharper(ExecutionEnvironment environment, IFile descriptorPath, IPackageRepository packageRepository)
        {
            lock (_lock)
            {
                if (_called)
                    return;
                TryCreateIntegrationService();
                TryRegisterProjectToResharper(() => environment, descriptorPath, packageRepository);
            }
        }

        static void TryRegisterProjectToResharper(Func<ExecutionEnvironment> environment, IFile descriptorPath, IPackageRepository packageRepository)
        {
            if (_instance == null)
                Queue(environment, descriptorPath, packageRepository);
            else
                ExecuteBootstrapSolutionMethod(environment, descriptorPath, packageRepository);
        }

        static void Queue(Func<ExecutionEnvironment> environment, IFile descriptorPath, IPackageRepository packageRepository)
        {
            ResharperLogger.Debug("Queue: queued registration");

            _queuedRegistration = new NotificationRegistration(environment, descriptorPath, packageRepository);
        }

        static void TryCreateIntegrationService()
        {
            try
            {
                if (_instance != null)
                {
                    ResharperLogger.Debug("TryCreateIntegrationService: _instance already set.");
                    return;
                }
                var unimportantType = Type.GetType("JetBrains.Application.Shell, JetBrains.Platform.ReSharper.Shell");
                if (unimportantType == null && _tries < 50)
                {
                    ResharperLogger.Debug("TryCreateIntegrationService: resharperType not loaded, try {0}.", _tries.ToString());

                    _tries++;
                    _timer = new Timer(x => TryCreateIntegrationService(), null, WAIT_RETRY_MS, Timeout.Infinite);
                }
                if (unimportantType == null)
                    return;

                var installedVersion = unimportantType.Assembly.GetName().Version;
                var resharperIntegratorTypeName = (from supportedResharperVersion in _integrationTypes.Keys
                                               orderby supportedResharperVersion descending
                                               where installedVersion >= supportedResharperVersion
                                               select _integrationTypes[supportedResharperVersion]).FirstOrDefault();

                if (resharperIntegratorTypeName == null) return;

                var resharperIntegratorType = Type.GetType(resharperIntegratorTypeName, false);
                if (resharperIntegratorType == null) return;
                _instance = Activator.CreateInstance(resharperIntegratorType);
                ResharperLogger.Debug("TryCreateIntegrationService: instance loaded: '{0}'.", _instance.GetType().AssemblyQualifiedName);
                if (_queuedRegistration != null)
                {
                    ExecuteBootstrapSolutionMethod
                        (
                            _queuedRegistration.Environment,
                            _queuedRegistration.DescriptorPath,
                            _queuedRegistration.PackageRepository
                        );
                }
            }
            finally
            {
                _called = true;
            }
        }

        static void ExecuteBootstrapSolutionMethod(
            Func<ExecutionEnvironment> environment,
            IFile descriptorPath,
            IPackageRepository packageRepository)
        {
            ResharperLogger.Debug("ExecuteBootstrapSolutionMethod: executing.");

            _instance.GetType().GetMethod("BootstrapSolution").Invoke(_instance, new object[] { environment, packageRepository });
        }

        class NotificationRegistration
        {
            public Func<ExecutionEnvironment> Environment { get; set; }
            public IFile DescriptorPath { get; set; }
            public IPackageRepository PackageRepository { get; set; }

            public NotificationRegistration(Func<ExecutionEnvironment> environment, IFile descriptorPath, IPackageRepository packageRepository)
            {
                Environment = environment;
                DescriptorPath = descriptorPath;
                PackageRepository = packageRepository;
            }
        }
    }
}
