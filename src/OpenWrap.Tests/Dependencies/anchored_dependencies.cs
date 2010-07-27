using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenRasta.Wrap.Tests.Dependencies.context;

namespace OpenWrap.Tests.Dependencies
{
    public class when_dependency_is_anchored : dependency_manager_context
    {
        public when_dependency_is_anchored()
        {
            given_dependency("depends: one-ring anchored");
            given_system_package("one-ring-1.0.0");

            when_resolving_packages();
        }
        [Test]
        public void folder_is_created_without_version_number()
        {
            
        }
    }
}
