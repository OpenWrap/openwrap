using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using OpenWrap.Dependencies;
using OpenWrap.Exports;

namespace OpenWrap.Repositories
{
    public static class GacResolver
    {
        public class Loader : MarshalByRefObject
        {
            public bool InGAC(AssemblyName assemblyName)
            {
                return Assembly.LoadWithPartialName(assemblyName.Name) != null;
            }
        }

        public static ILookup<IPackageInfo, AssemblyName> InGac(IEnumerable<IPackageInfo> packages, ExecutionEnvironment environment)
        {
            var domain = TempDomain();
            var loader = ((Loader)domain.CreateInstanceAndUnwrap(
                    typeof(Loader).Assembly.FullName, 
                    typeof(Loader).FullName));
            try
            {
                return (from package in packages.NotNull().Select(x=>x.Load()).NotNull()
                        let export = package.GetExport("bin", environment)
                        where export != null
                        from assembly in export.Items.OfType<IAssemblyReferenceExportItem>()
                        let inGac = loader.InGAC(assembly.AssemblyName)
                        where inGac
                        select new { package, assembly.AssemblyName })
                        .ToLookup(x => (IPackageInfo)x.package, x => x.AssemblyName);
            }
            finally
            {
                AppDomain.Unload(domain);
            }
        }

        static AppDomain TempDomain()
        {
            var currentSetup = AppDomain.CurrentDomain.SetupInformation;

            var setupCopy = new AppDomainSetup
            {
                    ApplicationBase = currentSetup.ApplicationBase
            };

            return AppDomain.CreateDomain("GAC Resolve",null,setupCopy);
        }
    }
}
