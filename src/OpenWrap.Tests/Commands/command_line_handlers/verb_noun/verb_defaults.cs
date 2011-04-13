using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Commands.command_line_handlers.verb_noun
{

    [TestFixture("get")]
    [TestFixture("list")]
    class verb_defaults : contexts.verb_noun_handler
    {
        readonly string _expectedVerb;

        public verb_defaults(string verb)
        {
            _expectedVerb = verb;
            given_command(verb, "ring");
            when_executing("ring");
        }

        [Test]
        public void command_is_found()
        {
            command_should_be(_expectedVerb,"ring");
        }

        [Test]
        public void verb_noun_removed_from_line()
        {
            ResultingLine.ShouldBe(string.Empty);
        }
    }
}
