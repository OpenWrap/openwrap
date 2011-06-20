using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Runtime;
using OpenWrap.Testing;
using Tests;

namespace Tests.framework_versioning
{
    public class from_dte : context
    {
        [TestCase(0x20000u, null, "net20")]
        [TestCase(0x30000u, null, "net30")]
        [TestCase(0x30005u, null, "net35")]
        [TestCase(0x40000u, ".NetFramework,Version=v4.0", "net40")]
        [TestCase(0x40000u, ".NetFramework, Version=v3.5, Profile=Client", "net35cp")]
        [TestCase(0x40000u, ".NetFramework, Version=v4.0, Profile=Client", "net40cp")]
        [TestCase(0x50000u, ".NetFramework, Version=v5.0, Profile=Client", null)]
        [TestCase(0x50000u, "Silverlight, Version=v3.0", "sl30")]
        [TestCase(0x50000u, "Silverlight, Version=v4.0", "sl40")]
        [TestCase(0x50000u, "Silverlight, Version=v4.0, Profile=WindowsPhone", "wp70")]
        public void version_is_valid(uint targetFramework, string frameworkMoniker, string expected)
        {
            TargetFramework.ParseDTEIdentifier(targetFramework, frameworkMoniker)
                .ShouldBe(TargetFramework.ParseOpenWrapIdentifier(expected));
        }
    }
 
}