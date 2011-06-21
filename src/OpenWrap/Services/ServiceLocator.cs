using System;
using System.Collections.Generic;
using OpenWrap.PackageManagement.Monitoring;

namespace OpenWrap.Services
{
    // Note, this class is *evil* and *will* be removed over time. It's a temporary solution only. :)
    public static class ServiceLocator
    {
        static readonly Dictionary<Type, Func<object>> _factories = new Dictionary<Type, Func<object>>();
        static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        static ServiceLocator()
        {
            Clear();
        }

        public static void Clear()
        {
            _services.Clear();
            _factories.Clear();
            RegisterService<IPackageDescriptorMonitor>(new PackageDescriptorMonitor());
        }

        public static T GetService<T>() where T : class
        {
            return _services.ContainsKey(typeof(T))
                           ? (T)_services[typeof(T)]
                           : (_factories.ContainsKey(typeof(T))
                                      ? (T)(_services[typeof(T)] = Instantiate<T>())
                                      : null);
        }

        static object Instantiate<T>()
        {
            var instance = _factories[typeof(T)]();
            var service = instance as IService;
            if (service != null)
                service.Initialize();
            return instance;
        }

        public static bool HasService<T>()
        {
            return _services.ContainsKey(typeof(T)) || _factories.ContainsKey(typeof(T));
        }

        public static void RegisterService<TService>(TService instance) where TService : class
        {
            _services[typeof(TService)] = instance;
            var service = instance as IService;
            if (service != null)
                service.Initialize();
        }

        public static void TryRegisterService<TService>(Func<TService> service) where TService : class
        {
            if (!_services.ContainsKey(typeof(TService)) && !_factories.ContainsKey(typeof(TService)))
            {
                _factories[typeof(TService)] = () => service();
            }
        }
    }
}