using System;
using System.Collections.Generic;
using OpenRasta.Wrap.Build;
using OpenRasta.Wrap.Build.Services;

namespace OpenRasta.Wrap.Build.Services
{
    public interface VSIService : IService {}
    public static class WrapServices
    {
        static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        static WrapServices()
        {
            var wrapFile = new WrapDescriptorMonitor();

            RegisterService<IWrapDescriptorMonitoringService>(wrapFile);
            
        }
        public static void RegisterService<TService>(TService service) where TService : class, IService
        {
            _services[typeof(TService)] = service;
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
    }
}