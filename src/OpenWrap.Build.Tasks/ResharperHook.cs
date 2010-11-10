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
using OpenWrap.Dependencies;
using OpenWrap.Repositories;
using OpenFileSystem.IO;
using System.Diagnostics;


namespace OpenWrap.Build.Tasks
{
    static class ResharperLogger
    {
        public static void Debug(string text, params string[] args)
        {
            Debugger.Log(0, "resharper", DateTime.Now.ToShortTimeString() +":"+ string.Format(text, args)+"\r\n");
        }
    }
    public static class ResharperHook
    {
        static Queue<NotificationRegistration> _registrationQueue = new Queue<NotificationRegistration>();
        static Dictionary<Version, Type> _integrationTypes = new Dictionary<Version, Type>
        {
            {new Version("4.5.1288.2"), typeof(resharper450::OpenWrap.Resharper.ResharperIntegrationService)},
            {new Version("5.0.1659.36"), typeof(resharper500::OpenWrap.Resharper.ResharperIntegrationService)},
            {new Version("5.1.1727.12"), typeof(resharper510::OpenWrap.Resharper.ResharperIntegrationService)},
            {new Version("5.1.1751.8"), typeof(resharper511::OpenWrap.Resharper.ResharperIntegrationService)}
        };

        static object _instance;
        static Timer _timer;
        static int _tries = 0;

        public static void TryRegisterResharper(ExecutionEnvironment environment, IFile descriptorPath, IPackageRepository packageRepository, string projectFilePath, IEnumerable<string> excludedAssemblies)
        {
            TryCreateIntegrationService();
            TryRegisterProjectToResharper(environment, descriptorPath, packageRepository, projectFilePath, excludedAssemblies);
        }

        static void TryRegisterProjectToResharper(ExecutionEnvironment environment, IFile descriptorPath, IPackageRepository packageRepository, string projectFilePath, IEnumerable<string> excludedAssemblies)
        {
            lock (_registrationQueue)
            {
                if (_instance == null)
                    Queue(environment, descriptorPath, packageRepository, projectFilePath, excludedAssemblies);
                else
                    ExecuteTryAddNotifierMethod(environment,descriptorPath,packageRepository,projectFilePath,excludedAssemblies);
            }
        }

        static void Queue(ExecutionEnvironment environment, IFile descriptorPath, IPackageRepository packageRepository, string projectFilePath, IEnumerable<string> excludedAssemblies)
        {
            ResharperLogger.Debug("Queue: queued '{0}'.", projectFilePath);

            _registrationQueue.Enqueue(new NotificationRegistration(environment,descriptorPath,packageRepository,projectFilePath, excludedAssemblies));
        }

        static void TryCreateIntegrationService()
        {
            lock (_registrationQueue)
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
                    _timer = new Timer(x => TryCreateIntegrationService(), null, 500, Timeout.Infinite);
                }
                if (unimportantType == null)
                    return;

                var installedVersion = unimportantType.Assembly.GetName().Version;
                var resharperIntegratorType = (from supportedResharperVersion in _integrationTypes.Keys
                                               orderby supportedResharperVersion descending
                                               where installedVersion >= supportedResharperVersion
                                               select _integrationTypes[supportedResharperVersion]).FirstOrDefault();

                if (resharperIntegratorType == null) return;

                _instance = Activator.CreateInstance(resharperIntegratorType);
                ResharperLogger.Debug("TryCreateIntegrationService: instance loaded: '{0}'.", _instance.GetType().AssemblyQualifiedName);
                foreach(var reg in _registrationQueue)
                {
                    ExecuteTryAddNotifierMethod(
                            reg.Environment,
                            reg.DescriptorPath,
                            reg.PackageRepository,
                            reg.ProjectFilePath,
                            reg.ExcludedAssemblies
                            );
                }
                _registrationQueue.Clear();
            }
        }

        static void ExecuteTryAddNotifierMethod(ExecutionEnvironment environment, IFile descriptorPath, IPackageRepository packageRepository, string projectFilePath, IEnumerable<string> excludedAssemblies)
        {
            ResharperLogger.Debug("ExecuteTryAddNotifierMethod: executing TryAdd for {0}.", projectFilePath);

            _instance.GetType().GetMethod("TryAddNotifier").Invoke(_instance, new object[] { environment, descriptorPath, packageRepository, projectFilePath, excludedAssemblies });
        }

        class NotificationRegistration
        {
            public ExecutionEnvironment Environment { get; set; }
            public IFile DescriptorPath { get; set; }
            public IPackageRepository PackageRepository { get; set; }
            public string ProjectFilePath { get; set; }
            public IEnumerable<string> ExcludedAssemblies { get; set; }

            public NotificationRegistration(ExecutionEnvironment environment, IFile descriptorPath, IPackageRepository packageRepository, string projectFilePath, IEnumerable<string> excludedAssemblies)
            {
                Environment = environment;
                DescriptorPath = descriptorPath;
                PackageRepository = packageRepository;
                ProjectFilePath = projectFilePath;
                ExcludedAssemblies = excludedAssemblies;
            }
        }
    }
}
