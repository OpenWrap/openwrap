using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Commands.Cli.Locators;

namespace OpenWrap.Commands.Cli
{
    // NOTE: This class is temporary, don't rely on it, it will change
    public class ConsoleCommandExecutor
    {
        readonly IEnumerable<ICommandLocator> _handlers;
        readonly IEventHub _eventHub;
        readonly ICommandOutputFormatter _formatter;

        public ConsoleCommandExecutor(IEnumerable<ICommandLocator> handlers, IEventHub eventHub, ICommandOutputFormatter formatter)
        {
            _handlers = handlers;
            _eventHub = eventHub;
            _formatter = formatter;
        }

        public int Execute(string commandLine, IEnumerable<string> optionalInputs)
        {
            commandLine = commandLine.Trim();
            if (commandLine == string.Empty) return 0;
            string commandParameters = commandLine;
            var command = _handlers.Select(x => x.Execute(ref commandParameters)).Where(x => x != null).FirstOrDefault();
            if (command == null)
            {
                var sp = commandLine.IndexOf(" ");

                _formatter.Render(new Error("The term '{0}' is not a recognized command or alias. Check the spelling or enter 'get-help' to get a list of available commands.",
                           sp != -1 ? commandLine.Substring(0, sp) : commandLine));
                return -10;
            }
            int returnCode = 0;
            var commandLineRunner = new CommandLineRunner { OptionalInputs = optionalInputs };
            using(_eventHub.Subscribe<ICommandOutput>(_formatter.Render))
            foreach (var output in commandLineRunner.Run(command, commandParameters))
            {
                _eventHub.Publish(output);
                if (output.Type == CommandResultType.Error)
                {
                    returnCode = -50;
                }
            }
            return returnCode;
        }
    }

    public interface IEventHub
    {
        void Publish(object message);
        IDisposable Subscribe<T>(Action<T> handler);
    }

    public class EventHub : IEventHub
    {
        
        Dictionary<Type, List<Action<object>>> _handlers = new Dictionary<Type, List<Action<object>>>();
        public void Publish(object message)
        {
            List<Type> types = new List<Type>();
            var currentType = message.GetType();
            do
            {
                types.Add(currentType);

            } while ((currentType = currentType.BaseType) != null);
            types.AddRange(message.GetType().GetInterfaces());

                var actions = new List<List<Action<object>>>();
            lock(_handlers)
            {
                types.ForEach(_ => _handlers.TryGet(_, actions.Add));
            }
            var invokeList = new List<Action<object>>();
            foreach(var action in actions)
            {
                lock(action)
                {
                    invokeList.AddRange(action);
                }
            }
            var errors = new List<Exception>();
            foreach (var invoke in invokeList)
            {
                try
                {
                    invoke(message);

                }
                catch(Exception e)
                {
                    errors.Add(e);
                }
            }
            foreach(var error in errors) Publish(error);
        }
        public IDisposable Subscribe<T>(Action<T> handler)
        {
            List<Action<object>> list;
            lock(_handlers)
            {
                list = _handlers.GetOrCreate(typeof(T));
            }
            lock(list){
                Action<object> reg = message => handler((T)message);
                list.Add(reg);
                return new ActionOnDispose(() =>
                {
                    lock (list)
                    {
                        list.Remove(reg);
                    }
                });
            }
        }
    }
    public interface ICommandOutputFormatter
    {
        void Render(ICommandOutput output);
    }
    public class ConsoleCommandOutputFormatter : ICommandOutputFormatter
    {
        static IDisposable ColorFromOutput(ICommandOutput output)
        {
            switch (output.Type)
            {
                case CommandResultType.Error:
                    return ColoredText.Red;
                case CommandResultType.Warning:
                    return ColoredText.Yellow;
                case CommandResultType.Verbose:
                    return ColoredText.Gray;
            }
            return new ActionOnDispose(() => { });
        }

        static void WriteError(string message, params string[] args)
        {
            using (ColoredText.Red)
                Console.WriteLine(message, args);
        }

        public void Render(ICommandOutput output)
        {
            using (ColorFromOutput(output))
                Console.WriteLine(output.ToString());
        }
    }
}