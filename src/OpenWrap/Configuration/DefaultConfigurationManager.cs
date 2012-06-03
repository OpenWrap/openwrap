using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using OpenFileSystem.IO;
using OpenRasta.Client;
using OpenWrap.Configuration.Persistence;
using OpenWrap.IO;
using OpenWrap.Reflection;

namespace OpenWrap.Configuration
{
    public class DefaultConfigurationManager : IConfigurationManager
    {
        public DefaultConfigurationManager(IDirectory configurationDirectory)
        {
            ConfigurationDirectory = configurationDirectory;
        }

        public IDirectory ConfigurationDirectory { get; private set; }

        public static IEnumerable<KeyValuePair<string, string>> ParseKeyValuePairs(string input)
        {
            const int KEY = 0;
            const int BEFORE_VAL = 1;
            const int QVALUE = 2;
            const int QKEY = 3;
            const int VALUE = 4;
            const int NONE = -1;
            int state = NONE;
            string key = null;
            string value = null;
            var sb = new StringBuilder();

            char curChar = '\0';
            Action append = () => sb.Append(curChar);
            Action commitKey = () =>
            {
                key = sb.ToString();
                if (state == KEY) key = key.Trim();
                sb = new StringBuilder();
                state = BEFORE_VAL;
            };
            Func<KeyValuePair<string, string>> commitVal = () =>
            {
                value = sb.ToString();
                if (state == VALUE) value = value.Trim();
                sb = new StringBuilder();

                state = NONE;

                var kv = GetKeyValue(key, value);
                key = null;
                return kv;
            };

            for (int pos = 0; pos < input.Length; pos++)
            {
                curChar = input[pos];
                if (curChar == '=')
                {
                    if (state == KEY) commitKey();
                    else if (state == QKEY || state == QVALUE || state == VALUE) append();
                }
                else if (curChar == ';')
                {
                    if (state == QKEY || state == QVALUE) append();
                    else if (state == KEY)
                    {
                        commitKey();
                        yield return commitVal();
                    }
                    else if (state == VALUE || state == BEFORE_VAL)
                        yield return commitVal();
                }
                else if (curChar == '\\')
                {
                    if ((state == QKEY || state == QVALUE) && pos < input.Length - 1) sb.Append(input[++pos]);
                    else if (state == KEY || state == VALUE) append();
                }
                else if (curChar == '"')
                {
                    if (state == QVALUE) yield return commitVal();
                    else if (state == BEFORE_VAL) state = QVALUE;
                    else if (state == QKEY || state == VALUE) commitKey();
                    else if (state == NONE)
                    {
                        state = QKEY;
                    }
                }
                else if (curChar == ' ')
                {
                    if (state == QVALUE || state == KEY || state == QKEY || state == VALUE) append();
                }
                else
                {
                    if (state == NONE) state = KEY;
                    else if (state == BEFORE_VAL) state = VALUE;
                    append();
                }
            }

            if (sb.Length > 0 && key == null)
                key = sb.ToString();
            else if (sb.Length > 0)
                value = sb.ToString();
            if (key != null)
                yield return GetKeyValue(key, value);
        }


        public T Load<T>(Uri uri = null) where T : new()
        {
            uri = uri ?? DetermineUriFromAttribute<T>();
            if (uri == null) throw new InvalidOperationException("The configuration does not contain any Path attribute and no uri was provided.");
            var file = GetConfigurationFile(uri);
            if (!file.Exists)
                return GetDefaultValueFor<T>();
            return ReadFile<T>(file);
        }

        public void Save<T>(T configEntry, Uri uri = null)
        {
            if (configEntry == null) return;
            uri = uri ?? DetermineUriFromAttribute<T>();

            var configFile = GetConfigurationFile(uri);
            using (var writer = new StreamWriter(configFile.OpenWrite()))
            {
                WriteProperties(configEntry, writer.WriteProperty);

                var dictionaryInterface = FindDictionaryInterface<T>();
                if (dictionaryInterface != null)
                {
                    WriteDictionaryEntries(writer, dictionaryInterface, configEntry);
                }
            }
        }

        static object AssignPropertiesFromLines(object instance, IEnumerable<ConfigurationLine> lines)
        {
            var type = instance.GetType();
            var propertiesWithKeys = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(prop => new { prop, attr = prop.Attribute<KeyAttribute>() })
                .Where(@t => t.attr != null)
                .ToLookup(x => x.attr.Name, x => x.prop, StringComparer.OrdinalIgnoreCase);

            foreach (var linesByName in lines.GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase))
            {
                var property = type.GetProperty(linesByName.Key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase)
                               ?? (propertiesWithKeys.Contains(linesByName.Key) ? propertiesWithKeys[linesByName.Key].FirstOrDefault() : null);

                if (property == null || property.Attribute<IgnoreAttribute>() != null || HasGetterAndSetter(property) == false) continue;
                object propertyValue;
                if (PropertyIsList(property.PropertyType))
                {
                    var itemType = property.PropertyType.GetGenericArguments()[0];
                    var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType));
                    foreach (var obj in linesByName.Select(x => AssignPropertyFromLine(itemType, x.Value, property.Attribute<EncryptAttribute>() != null)))
                        list.Add(obj);
                    propertyValue = list;
                }
                else
                {
                    propertyValue = AssignPropertyFromLine(property.PropertyType, linesByName.Last().Value, property.Attribute<EncryptAttribute>() != null);
                }

                property.SetValue(instance, propertyValue, null);
            }

            return instance;
        }

        static object AssignPropertyFromLine(Type targetType, string value, bool encrypted)
        {
            object propertyValue;
            value = value.DecodeBreaks();

            var keyValues = ParseKeyValuePairs(value);

            if (encrypted)
            {
                // there can only be one value anyways, need to re-parse the data
                try
                {
                    var key = keyValues.First();
                    var decrypted = Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(key.Key), null, DataProtectionScope.CurrentUser));
                    keyValues = ParseKeyValuePairs(decrypted);
                }
                catch
                {
                    return null;
                }
            }

            var firstKey = keyValues.FirstOrDefault();
            var properties = keyValues.AsEnumerable();
            if (firstKey.Key != null && firstKey.Value == null)
            {
                string rootValue = firstKey.Key;
                propertyValue = targetType.CreateInstanceFrom(rootValue);
                properties = properties.Skip(1);
            }
            else
            {
                propertyValue = Activator.CreateInstance(targetType);
            }

            if (targetType.IsPrimitive == false && properties.Any())
                propertyValue = AssignPropertiesFromLines(propertyValue, 
                                                          properties
                                                              .Where(_ => _.Value != null)
                                                              .Select(x => new ConfigurationLine
                                                              {
                                                                  Name = x.Key, 
                                                                  Value = new StringBuilder().AppendQuoted(x.Value).ToString()
                                                              }));
            return propertyValue;
        }

        static Uri DetermineUriFromAttribute<T>()
        {
            var attrib = typeof(T).Attribute<PathAttribute>();
            if (attrib == null) return null;

            return ConstantUris.Base.Combine(attrib.Uri);
        }

        static IEnumerable<object> Enumerate(object values)
        {
            var enumerable = values as IEnumerable;
            if (values == null || ((values is string) == false && enumerable != null && enumerable.GetEnumerator().MoveNext() == false))
                yield break;


            if (enumerable != null && (values is string) == false)
            {
                foreach (var val in enumerable) yield return val;
            }
            else
            {
                yield return values;
            }
        }

        static Type FindDictionaryInterface<T>()
        {
            return typeof(T).GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDictionary<,>) && x.GetGenericArguments()[0] == typeof(string));
        }

        static T GetDefaultValueFor<T>()
        {
            var pi = typeof(T).GetProperty("Default", BindingFlags.Static | BindingFlags.Public);
            var fi = typeof(T).GetField("Default", BindingFlags.Static | BindingFlags.Public);
            if (pi == null)
                if (fi == null)
                    return default(T);
                else
                    // ReSharper disable AssignNullToNotNullAttribute
                    return (T)fi.GetValue(null);
            // ReSharper restore AssignNullToNotNullAttribute
            return (T)pi.GetValue(null, null);
        }

        static KeyValuePair<string, string> GetKeyValue(string key, string value)
        {
            return new KeyValuePair<string, string>(key, value != null && value.Length > 0 ? value : null);
        }

        static bool HasGetterAndSetter(PropertyInfo pi)
        {
            var setter = pi.GetSetMethod();
            var getter = pi.GetGetMethod();
            return getter != null && setter != null && getter.IsPublic && setter.IsPublic;
        }

        static void PopulateDictionaryEntries<T>(IFile file, Type dictionaryInterface, IEnumerable<ConfigurationEntry> parsedConfig, T configData)
        {
            var dictionaryParameterType = dictionaryInterface.GetGenericArguments()[1];
            var addMethod = dictionaryInterface.GetMethod("Add", new[] { typeof(string), dictionaryParameterType });

            foreach (var section in parsedConfig.OfType<ConfigurationSection>().Where(x => x.Type.EqualsNoCase(dictionaryParameterType.Name)))
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
                    throw e.InnerException;
                }
            }
        }

        static bool PropertyIsList(Type propertyType)
        {
            if (!propertyType.IsGenericType) return false;
            var typeDef = propertyType.GetGenericTypeDefinition();
            return typeDef == typeof(IEnumerable<>) ||
                   typeDef == typeof(IList<>) ||
                   typeDef == typeof(ICollection<>) ||
                   typeDef == typeof(List<>);
        }

        static T ReadFile<T>(IFile file) where T : new()
        {
            string data = file.ReadRetry(stream => stream.ReadString());

            var parsedConfig = new ConfigurationParser().Parse(data);
            var configData = new T();

            var dictionaryInterface = FindDictionaryInterface<T>();
            if (dictionaryInterface != null)
            {
                PopulateDictionaryEntries(file, dictionaryInterface, parsedConfig, configData);
            }

            AssignPropertiesFromLines(configData, parsedConfig.OfType<ConfigurationLine>());
            return configData;
        }

        static void WriteDictionaryEntries<T>(StreamWriter configFile, Type dictionaryInterface, T configEntry)
        {
            var entryType = dictionaryInterface.GetGenericArguments()[1];
            var kvPairType = typeof(KeyValuePair<,>).MakeGenericType(typeof(string), entryType);
            var kvKey = kvPairType.GetProperty("Key");
            var kvValue = kvPairType.GetProperty("Value");

            var kvInterface = typeof(IEnumerable<>).MakeGenericType(kvPairType);

            var enumerator = (IEnumerator)kvInterface.GetMethod("GetEnumerator").Invoke(configEntry, new object[0]);
            Func<object, string> keyReader = x => (string)kvKey.GetValue(x, null);
            Func<object, object> valueReader = x => kvValue.GetValue(x, null);

            while (enumerator.MoveNext())
            {
                var key = keyReader(enumerator.Current);
                var value = valueReader(enumerator.Current);

                WriteSection(configFile, entryType.Name, key, value);
            }
        }

        static void WriteProperties(object target, Action<string, string> writer)
        {
            var properties = from pi in target.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                             where pi.Attribute<IgnoreAttribute>() == null && HasGetterAndSetter(pi)
                             let keyAttrib = pi.Attribute<KeyAttribute>()
                             let name = keyAttrib != null ? keyAttrib.Name : pi.Name
                             where pi.GetIndexParameters().Length == 0
                             let encryptAttrib = pi.Attribute<EncryptAttribute>()
                             let values = pi.GetValue(target, null)
                             where values != null
                             from value in Enumerate(values)
                             select new { name, value = WriteProperty(encryptAttrib, value) };

            foreach (var property in properties)
                writer(property.name.ToLowerInvariant(), property.value);
        }

        static string WriteProperty(EncryptAttribute encryptAttrib, object value)
        {
            var builder = new StringBuilder();
            if (value.ToString() != value.GetType().ToString())
                builder.AppendQuoted(value.ToString().EncodeBreaks());
            WriteProperties(value, 
                            (key, val) => builder.Append(builder.Length > 0 ? "; " : string.Empty)
                                              .AppendQuoted(key)
                                              .Append("=")
                                              .Append(val.EncodeBreaks()));

            return encryptAttrib != null
                       ? new StringBuilder().AppendQuoted(
                           Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(builder.ToString()), null, DataProtectionScope.CurrentUser)))
                             .ToString()
                       : builder.ToString();
        }

        static void WriteSection(StreamWriter configFile, string sectionName, string key, object value)
        {
            configFile.WriteSection(sectionName, key);
            WriteProperties(value, configFile.WriteProperty);
        }

        IFile GetConfigurationFile(Uri uri)
        {
            return ConfigurationDirectory.GetFile(uri.IsAbsoluteUri ? ConstantUris.Base.MakeRelativeUri(uri).ToString() : uri.ToString());
        }
    }
}