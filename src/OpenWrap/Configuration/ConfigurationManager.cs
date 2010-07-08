#region
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using OpenWrap.IO;

#endregion

namespace OpenWrap.Configuration
{
    public class ConfigurationManager : IConfigurationManager
    {
        readonly IDirectory _configurationDirectory;

        public ConfigurationManager(IDirectory configurationDirectory)
        {
            _configurationDirectory = configurationDirectory;
            BaseUri = Configurations.Addresses.BaseUri;
        }

        public Uri BaseUri { get; private set; }

        public T Load<T>(Uri uri) where T : new()
        {
            var relativeUri = Configurations.Addresses.BaseUri.MakeRelativeUri(uri).ToString();
            var file = _configurationDirectory.FindFile(relativeUri);
            if (file == null)
                return GetDefaultValueFor<T>();
            return ParseFile<T>(file);
        }

        T GetDefaultValueFor<T>()
        {
            var pi = typeof(T).GetProperty("Default", BindingFlags.Static | BindingFlags.Public);
            var fi = typeof(T).GetField("Default", BindingFlags.Static | BindingFlags.Public);
            if (pi == null)
                if (fi == null)
                    return default(T);
                else 
                    return (T)fi.GetValue(null);

            return (T)pi.GetValue(null, null);
        }

        public void Save<T>(Uri uri, T configEntry)
        {
            
        }

        static object AssignPropertiesFromLines(object instance, IEnumerable<ConfigurationLine> lines)
        {
            var type = instance.GetType();

            foreach (var line in lines)
            {
                var property = type.GetProperty(line.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (property == null)
                    continue;
                var propertyValue = property.PropertyType.CreateInstanceFrom(line.Value);

                property.SetValue(instance, propertyValue, null);
            }
            return instance;
        }

        static T ParseFile<T>(IFile file) where T : new()
        {
            string data;
            using (var fileStream = file.OpenRead())
                data = Encoding.UTF8.GetString(fileStream.ReadToEnd());

            var parsedConfig = new ConfigurationParser().Parse(data);
            var configData = new T();

            var dictionaryInterface =
                typeof(T).GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDictionary<,>) && x.GetGenericArguments()[0] == typeof(string));
            if (dictionaryInterface != null)
            {
                var dictionaryParameterType = dictionaryInterface.GetGenericArguments()[1];
                var addMethod = dictionaryInterface.GetMethod("Add", new[] { typeof(string), dictionaryParameterType });

                foreach (var section in parsedConfig.OfType<ConfigurationSection>().Where(x => x.Type.Equals(dictionaryParameterType.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    try
                    {
                        addMethod.Invoke(configData, new[] { section.Name, AssignPropertiesFromLines(Activator.CreateInstance(dictionaryParameterType), section.Lines) });
                    }
                    catch (TargetInvocationException e)
                    {
                        if (e.InnerException is ArgumentException)
                            throw new InvalidConfigurationException(
                                string.Format("Duplicate configuration section of type '{0}' with name '{1} in the file '{2}' found. Correct the issue and retry.",
                                              section.Type,
                                              section.Name,
                                              file.Path.FullPath),
                                e.InnerException);
                        else
                            throw;
                    }
                }
            }
            return configData;
        }
    }
}