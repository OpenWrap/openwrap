extern alias resharper450;
extern alias resharper500;
extern alias resharper510;
extern alias resharper511;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;
using OpenFileSystem.IO;


namespace OpenWrap.Build.Tasks
{

    public static class ResharperHook
    {
        static Dictionary<Version, Type> _integrationTypes = new Dictionary<Version, Type>
        {
            {new Version("4.5.1288.2"), typeof(resharper450::OpenWrap.Resharper.ResharperIntegrationService)},
            {new Version("5.0.1659.36"), typeof(resharper500::OpenWrap.Resharper.ResharperIntegrationService)},
            {new Version("5.1.1727.12"), typeof(resharper510::OpenWrap.Resharper.ResharperIntegrationService)},
            {new Version("5.1.1751.8"), typeof(resharper511::OpenWrap.Resharper.ResharperIntegrationService)}
        };
        public static object TryRegisterResharper(ExecutionEnvironment environment, IFile descriptorPath, IPackageRepository packageRepository, string projectFilePath, IEnumerable<string> excludedAssemblies)
        {
            var unimportantType = Type.GetType("JetBrains.Application.Shell, JetBrains.Platform.ReSharper.Shell");
            if (unimportantType == null) return null;

            var installedVersion = unimportantType.Assembly.GetName().Version;
            var resharperIntegratorType = (from supportedResharperVersion in _integrationTypes.Keys
                                            orderby supportedResharperVersion descending
                                            where installedVersion >= supportedResharperVersion
                                            select _integrationTypes[supportedResharperVersion]).FirstOrDefault();

            if (resharperIntegratorType == null) return null;

            var instance = Activator.CreateInstance(resharperIntegratorType, environment);
            resharperIntegratorType.GetMethod("TryAddNotifier").Invoke(instance, new object[] { descriptorPath, packageRepository, projectFilePath, excludedAssemblies });
            return instance;
        }
    }
}
