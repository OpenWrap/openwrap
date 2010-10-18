using System;
using System.Reflection;

namespace OpenWrap.Repositories
{
    public static class GACResolve
    {
        public class Loader : MarshalByRefObject
        {
            public bool InGAC(string partialName)
            {
                return Assembly.LoadWithPartialName(partialName) != null;
            }
        }

        public static bool InGAC(ResolvedDependency dependency)
        {
            var domain = TempDomain();
            try
            {
                return ((Loader)domain.CreateInstanceAndUnwrap(
                    typeof(Loader).Assembly.FullName, 
                    typeof(Loader).FullName)).InGAC(dependency.Dependency.Name);
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
