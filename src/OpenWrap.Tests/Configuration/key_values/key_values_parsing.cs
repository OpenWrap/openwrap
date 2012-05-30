using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenWrap.Configuration;
using OpenWrap.Testing;

namespace Tests.Configuration.key_values
{
    public class key_values_parsing : context
    {
        const string long_parse =
            @"token=""tok=en""; username=username; password=""AQAAANCMnd8BFdERjHoAwE/Cl+sBAAAAw1XrLa7LDEmN1mZMPENFcAAAAAACAAAAAAADZgAAwAAAABAAAADvsEEiXUlJuSaQgKsCPna/AAAAAASAAACgAAAAEAAAABrQ89nc4+f2WKEVDrIQKaUQAAAADvZaHxZhsNmtdYFD/WrzEBQAAAByvwtl4MNS62Y2erPwbNGFh6OpBA==""";

        [Test]
        public void parse_flags()
        {
            parsed("Sam;Frodo")
                .Check(_ => _["Sam"].ShouldBeNull())
                .Check(_ => _["Frodo"].ShouldBeNull());
        }

        [TestCase("Name=Froddo", "Name", "Froddo")]
        [TestCase("initial;key=value;Name=Fro\\ddo", "Name", "Fro\\ddo")]
        [TestCase("initial;key=value;Name=\"Fro\\ddo\"", "Name", "Froddo")]
        [TestCase("Name = Froddo", "Name", "Froddo")]
        [TestCase("Name = Froddo=", "Name", "Froddo=")]
        [TestCase("\"Name\" = Froddo", "Name", "Froddo")]
        [TestCase("Name=\"Froddo\"", "Name", "Froddo")]
        [TestCase("Name=\"Froddo; Baggins = cool\"", "Name", "Froddo; Baggins = cool")]
        [TestCase(long_parse, "token", "tok=en")]
        [TestCase(long_parse, "username", "username")]
        
        public void parse_properties(string input, string key, string value)
        {
            parsed(input)[key].ShouldBe(value);
        }
        [Test]
        public void parse_key_without_value()
        {
            parsed("Name=;Description=hello")["Name"].ShouldBe(null);
            
        }
        [TestCase("Froddo", null)]
        [TestCase("\"Froddo\"", "Froddo")]
        [TestCase("Froddo Baggins", null)]
        [TestCase("\"Froddo Baggins\"", "Froddo Baggins")]
        [TestCase("\"Froddo; Sam\"", "Froddo; Sam")]
        [TestCase(@"""Froddo \""Baggins\"" """, "Froddo \"Baggins\" ")]
        public void parse_single_value(string input, string expected)
        {
            parsed(input)[expected ?? input].ShouldBeNull();
        }

        static Dictionary<string, string> parsed(string input)
        {
            return DefaultConfigurationManager.ParseKeyValuePairs(input).ToDictionary(x => x.Key, x => x.Value);
        }
    }
}