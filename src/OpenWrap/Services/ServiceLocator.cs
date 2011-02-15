﻿using System;
using System.Collections.Generic;
using OpenWrap.PackageManagement.Monitoring;

namespace OpenWrap.Services
{
    public static class ServiceLocator
    {
        static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        static ServiceLocator()
        {
            Clear();
        }

        public static void Clear()
        {
            _services.Clear();
            RegisterService<IPackageDescriptorMonitor>(new PackageDescriptorMonitor());
        }

        public static T GetService<T>() where T : class
        {
            return _services.ContainsKey(typeof(T)) ? (T)_services[typeof(T)] : null;
        }

        public static bool HasService<T>()
        {
            return _services.ContainsKey(typeof(T));
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
            if (!_services.ContainsKey(typeof(TService)))
            {
                RegisterService(service());
            }
        }
    }
}