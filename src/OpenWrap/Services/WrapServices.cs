using System;
using System.Collections.Generic;
using OpenWrap.Build;
using OpenWrap.Build.Services;

namespace OpenWrap.Build.Services
{
    public interface VSIService : IService {}
    public static class WrapServices
    {
        static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        static WrapServices()
        {
            Clear();
        }
        public static void RegisterService<TService>(TService instance) where TService : class
        {
            _services[typeof(TService)] = instance;
            var service = instance as IService;
            if (service != null)
                service.Initialize();
        }
        public static void TryRegisterService<TService>(Func<TService> service) where TService: class, IService
        {
            if (!_services.ContainsKey(typeof(TService)))
            {
                RegisterService<TService>(service());
            }
        }
        public static bool HasService<T>()
        {
            return _services.ContainsKey(typeof(T));
        }
        public static T GetService<T>() where T : class, IService
        {
            return _services.ContainsKey(typeof(T)) ? (T)_services[typeof(T)] : null;
        }

        public static void Clear()
        {
            _services.Clear();
            RegisterService<IWrapDescriptorMonitoringService>(new WrapDescriptorMonitor());
        }
    }
}