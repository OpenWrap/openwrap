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
    internal abstract class command_handler : context
    {
        protected CommandRepository Repository;
        protected ICommandLineHandler Handler;
        protected ICommandDescriptor Result;
        protected string ResultingLine;

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
    abstract class noun_verb_handler : command_handler
    {
        public noun_verb_handler()
        {
            
            Repository = new CommandRepository();
            Handler = new NounVerbCommandLineHandler(Repository);

        }
    }
    abstract class verb_noun_handler : command_handler
    {
        public verb_noun_handler()
        {
            Repository = new CommandRepository();
            Handler = new VerbNounCommandLineHandler(Repository);
        }
    }
}
