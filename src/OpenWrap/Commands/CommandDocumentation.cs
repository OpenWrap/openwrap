using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace OpenWrap.Commands
{
    public static class CommandDocumentation
    {
        static readonly Dictionary<Assembly, ResourceManager> _resourceManagers = new Dictionary<Assembly, ResourceManager>();

        public static string GetCommandDescription(Type commandType, string resourceKey)
        {
            ResourceManager manager;
            if (!_resourceManagers.TryGetValue(commandType.Assembly, out manager))
                _resourceManagers.Add(commandType.Assembly, manager = new ResourceManager(commandType.Assembly.GetName().Name + ".CommandDocumentation", commandType.Assembly) { IgnoreCase = true });
            try
            {
                return manager.GetString(resourceKey, CultureInfo.CurrentUICulture);
            }
            catch
            {
                return null;
            }
        }
    }
}