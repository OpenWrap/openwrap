using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace OpenWrap.Commands
{
    public static class CommandDocumentation
    {
        static readonly Dictionary<Assembly, ResourceManager> _resourceManagers = new Dictionary<Assembly, ResourceManager>();

        public static string GetCommandDescription(string resourceKey)
        {
            ResourceManager manager;
            var assembly = typeof(CommandDocumentation).Assembly;

            if (!_resourceManagers.TryGetValue(assembly, out manager))
                _resourceManagers.Add(assembly, manager = new ResourceManager(assembly.GetName().Name + ".CommandDocumentation", assembly) { IgnoreCase = true });
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