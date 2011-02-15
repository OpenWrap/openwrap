using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenWrap.Commands.Cli
{
    public interface ICommandLineHandler
    {
        IEnumerable<ICommandOutput> Parse(IEnumerable<string> args);
    }
    public abstract class CommandLineHandler
    {
        protected ICommandRepository Commands { get; private set; }

        public CommandLineHandler(ICommandRepository commands)
        {
            Commands = commands;
        }
    }
    public class NounVerbCommandLineHandler : CommandLineHandler
    {
        public NounVerbCommandLineHandler(ICommandRepository commands) : base(commands)
        {
        }
    }
    public class VerbNounCommandLineHandler : CommandLineHandler
    {
        public VerbNounCommandLineHandler(ICommandRepository commands) : base(commands)
        {
        }
    }
    public class AliasCommandLineHandler
    {
        
    }
}
