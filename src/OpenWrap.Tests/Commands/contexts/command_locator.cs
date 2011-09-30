using System;
using OpenWrap.Commands;
using OpenWrap.Commands.Cli.Locators;
using OpenWrap.Testing;

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
}