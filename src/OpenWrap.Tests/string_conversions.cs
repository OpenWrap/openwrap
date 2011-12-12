using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.PackageManagement.Exporters;
using OpenWrap.Testing;
using Tests;

namespace Tests
{
    public class string_conversions : context
    {
        [Test]
        public void can_convert_single_string()
        {
            can_convert<string>("value").ShouldBe("value");
        }

        [Test]
        public void cannot_convert_multiple_values_to_single_string()
        {
            cannot_convert<string>("value", "value2");
        }
        [Test]
        public void can_convert_to_semver()
        {
            can_convert<SemanticVersion>("1.0.0").ShouldBe(new SemanticVersion(1, 0, 0));

        }
        [Test]
        public void can_convert_to_version()
        {
            can_convert<Version>("1.0").ShouldBe(new Version(1, 0));
        }

        [Test]
        public void cannot_convert_invalid_version()
        {
            cannot_convert<Version>();
            cannot_convert<Version>("1");
            cannot_convert<Version>("1", "0");
        }
        [Test]
        public void can_convert_to_bool()
        {
            can_convert<bool>("true").ShouldBeTrue();
            can_convert<bool>("false").ShouldBeFalse();
        }

        [Test]
        public void can_convert_to_enum_of_string()
        {
            can_convert<IEnumerable<string>>("value1", "value2")
                    .ShouldBe("value1", "value2");
        }

        [Test]
        public void can_convert_to_enum_of_bools()
        {
            can_convert<IEnumerable<bool>>("true", "false")
                    .ShouldBe(true, false);
        }
        T can_convert<T>(params string[] values)
        {
            object result;
            StringConversion.TryConvert(typeof(T), values, out result).ShouldBeTrue();
            return (T)result;
        }
        void cannot_convert<T>(params string[] values)
        {
            object result;
            StringConversion.TryConvert(typeof(T), values, out result).ShouldBeFalse();
        }
    }
}