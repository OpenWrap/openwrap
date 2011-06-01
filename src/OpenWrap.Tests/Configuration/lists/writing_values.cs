using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenWrap.Configuration;
using OpenWrap.IO;
using OpenWrap.Testing;

namespace Tests.Configuration.lists
{
    public class writing_values : contexts.configuration<writing_values.ConfigSection>
    {
        public writing_values()
        {
            given_configuration(new ConfigSection{Values={"one", "two"}});
            when_saving_configuration("config");
        }

        [Test]
        public void values_are_round_tripped()
        {
            Entry.Values.ElementAt(0).ShouldBe("one");
            Entry.Values.ElementAt(1).ShouldBe("two");
        }

        [Test]
        public void values_are_written_one_per_line()
        {
            ConfigurationDirectory.FindFile("config")
                .ShouldNotBeNull()
                .OpenRead().ReadString(Encoding.UTF8)
                .ShouldContain("value: one\r\n").ShouldContain("value: two\r\n");
        }
        public class ConfigSection
        {
            public ConfigSection()
            {
                Values = new List<string>();
            }
            [Key("value")]
            public ICollection<string> Values { get; set; }
        }
    }
}