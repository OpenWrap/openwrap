using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using OpenWrap.Repositories;

namespace OpenWrap.Commands.Core
{
    public class GACResolve
    {
        public class Loader : MarshalByRefObject
        {
            public bool InGAC(string partialName)
            {
                return Assembly.LoadWithPartialName(partialName) != null;
            }
        }

        public bool InGAC(ResolvedDependency dependency)
        {

            var currentSetup = AppDomain.CurrentDomain.SetupInformation;

            var setupCopy = new AppDomainSetup
            {
                    ApplicationBase = currentSetup.ApplicationBase
            };

            var domain = AppDomain.CreateDomain("GAC Resolve",null,setupCopy);
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


     
    }
}
