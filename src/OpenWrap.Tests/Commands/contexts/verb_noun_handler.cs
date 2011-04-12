using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using command_description_spec;
using OpenWrap.Commands;
using OpenWrap.Commands.Cli;
using OpenWrap.Testing;

namespace Tests.Commands.contexts
{
    class verb_noun_handler : context
    {
        CommandRepository Repository;
        VerbNounCommandLineHandler Handler;
        protected ICommandDescriptor Result;
        protected string ResultingLine;

        public verb_noun_handler()
        {
            Repository = new CommandRepository();
            Handler = new VerbNounCommandLineHandler(Repository);
        }
        protected void when_executing(string line)
        {

            this.Result = Handler.Execute(ref line);
            ResultingLine = line;
        }

        protected void given_command(string verb, string noun)
        {
            Repository.Add(new MemoryCommandDescriptor { Noun = noun, Verb = verb });
        }

        protected void command_should_be(string expectedVerb, string expectedNoun)
        {
            Result.Noun.ShouldBe(expectedNoun);
            Result.Verb.ShouldBe(expectedVerb);
        }
    }
}
