using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace OpenWrap.Commands
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandAttribute : Attribute
    {
        public string Noun { get; set; }
        public string Verb { get; set; }
        public string DisplayName { get; set; }
        string _description;
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }
    }
    public static class CommandDocumentation
    {
        static Dictionary<Assembly, ResourceManager> _resourceManagers = new Dictionary<Assembly, ResourceManager>();
        public static string GetCommandDescription(Type commandType, string resourceKey)
        {
            ResourceManager manager;
            if (!_resourceManagers.TryGetValue(commandType.Assembly, out manager))
                _resourceManagers.Add(commandType.Assembly, manager = new ResourceManager(commandType.Assembly.GetName().Name + ".CommandDocumentation", commandType.Assembly){IgnoreCase = true});
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