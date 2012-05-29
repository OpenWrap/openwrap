using System;
using System.Collections.Generic;

namespace OpenWrap.Commands.Cli
{
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
}