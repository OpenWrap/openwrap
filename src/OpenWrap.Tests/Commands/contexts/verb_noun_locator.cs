using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.Commands;
using OpenWrap.Commands.Cli;
using OpenWrap.Commands.Cli.Locators;
using OpenWrap.Testing;
using Tests.Commands.usage;

namespace Tests.Commands.contexts
{
    internal abstract class command_locator : context
    {
        protected CommandRepository Repository;
        protected ICommandLocator Handler;
        protected ICommandDescriptor Result;
        protected string ResultingLine;

        protected void when_executing(string line)
        {

            this.Result = Handler.Execute(ref line);
            ResultingLine = line;
        }

        protected void given_command(string verb, string noun, params Action<MemoryCommandDescriptor>[] configurators)
        {
            var command = new MemoryCommandDescriptor { Noun = noun, Verb = verb };
            foreach(var c in configurators) c(command);
            Repository.Add(command);
        }

        protected void command_should_be(string expectedVerb, string expectedNoun)
        {
            Result.Noun.ShouldBe(expectedNoun);
            Result.Verb.ShouldBe(expectedVerb);
        }
    }
    abstract class command_locator<T> : command_locator where T: ICommandLocator
    {
        public command_locator(Func<ICommandRepository, T> builder)
        {
            Repository = new CommandRepository();
            Handler = builder(Repository);
        }
    }
    abstract class noun_verb_locator : command_locator<NounVerbCommandLocator>
    {
        public noun_verb_locator() : base(_=>new NounVerbCommandLocator(_))
        {   
        }
    }
    abstract class verb_noun_locator : command_locator<VerbNounCommandLocator>
    {
        public verb_noun_locator()
            : base(_ => new VerbNounCommandLocator(_))
        {
        }
    }
}
