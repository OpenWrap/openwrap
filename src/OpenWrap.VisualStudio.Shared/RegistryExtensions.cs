using System;
using System.Linq;
using Microsoft.Win32;

namespace OpenWrap.VisualStudio.Hooks
{
    public static class RegistryExtensions
    {
        public static RegistryKey SubKey(this RegistryKey key, string subKeyName, string subKeyDefault, params Action<RegistryKey>[] subKeyActions)
        {
            RegistryKey subKey = key.GetSubKeyNames().Contains(subKeyName) 
                                         ? key.OpenSubKey(subKeyName)
                                         : key.CreateSubKey(subKeyName);
            if (subKeyDefault != null)
                subKey.SetValue(string.Empty, subKeyDefault);

            foreach (var subkeyAction in subKeyActions)
                subkeyAction(subKey);
            return key;
        }
        public static RegistryKey SubKey(this RegistryKey key, string subKeyName, params Action<RegistryKey>[] subKeyActions)
        {
            return key.SubKey(subKeyName, null, subKeyActions);
        }
        public static RegistryKey Value(this RegistryKey key, string name, object value)
        {
            key.SetValue(name, value);
            return key;
        }
        public static object Value(this RegistryKey key, string name)
        {
            return key.GetValue(name);
        }    
    }
}