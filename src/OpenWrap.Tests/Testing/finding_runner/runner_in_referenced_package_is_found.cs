using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.Testing;

namespace OpenWrap.Tests.Testing.finding_runner
{
    public class runner_in_referenced_package_is_found : contexts.testing
    {
        public runner_in_referenced_package_is_found()
        {
            
        }
    }
    namespace contexts
    {
        public abstract class testing : context
        {
            protected testing()
            {
                
            }
        }
    }
}
