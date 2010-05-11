using System;
using System.IO;
using System.Security.AccessControl;
using Microsoft.Win32;
using System.Linq;

namespace OpenWrap.Vs2008
{
    //public static class AddInInstaller
    //{
    //    public static bool RegisterAddInInUserHive()
    //    {
    //        var path = CopyAssemblyToAppData();
    //        var registeredType = Type.GetTypeFromProgID(ComRegistrations.ADD_IN_PROGID, null, false);
    //        if (registeredType != null) return true;

    //        var classes = Registry.CurrentUser.OpenSubKey("Software\\Classes", RegistryKeyPermissionCheck.ReadWriteSubTree);
    //        if (classes == null) return false;

    //        classes
    //            .SubKey(ComRegistrations.ADD_IN_PROGID, ComRegistrations.ADD_IN_PROGID, x => x
    //                .SubKey("CLSID", "{" + ComRegistrations.ADD_IN_GUID + "}")
    //            )
    //            .SubKey("CLSID\\{" + ComRegistrations.ADD_IN_GUID + "}", ComRegistrations.ADD_IN_PROGID, x => x
    //                .SubKey("InprocServer32","mscoree.dll", inproc => inproc
    //                    .Value("ThreadingModel","Both")
    //                    .Value("Class","OpenWrap.ComShim")
    //                    .Value("Assembly", typeof(ComShim).Assembly.GetName().FullName)
    //                    .Value("CodeBase", path)
    //                    .Value("RuntimeVersion","v2.0.50727")
    //                    .SubKey("1.0.0.0", version => version
    //                        .Value("Class","OpenWrap.ComShim")
    //                        .Value("Assembly", typeof(ComShim).Assembly.GetName().FullName)
    //                        .Value("CodeBase", path)
    //                        .Value("RuntimeVersion", "v2.0.50727")
    //                    )
    //                )
    //                .SubKey("ProgId", ComRegistrations.ADD_IN_PROGID)
    //                .SubKey("Implemented Categories\\{62C8FE65-4EBB-45E7-B440-6E39B2CDBF29}")
    //            )
    //        ;

    //        return true;
    //    }

    //    static string CopyAssemblyToAppData()
    //    {
    //        var destinationPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    //        var comPath = Path.Combine(Path.Combine(destinationPath, "OpenWrap"),"ComShim");

    //        if (!Directory.Exists(comPath))
    //            Directory.CreateDirectory(comPath);

    //        var assemblyPath = Path.Combine(comPath, "OpenWrap.ComShim.dll");
    //        var existingPath = typeof(ComShim).Assembly.Location;
    //        try
    //        {
    //            File.Copy(existingPath, assemblyPath, true);
    //        } 
    //        catch (Exception)
    //        {}
    //        return "file:///" + assemblyPath.Replace('\\', '/');
    //    }
    //}
    public static class RegistryExtensions
    {
        public static RegistryKey SubKey(this RegistryKey key, string subKeyName, string subKeyDefault, params Action<RegistryKey>[] subKeyActions)
        {
            RegistryKey subKey = key.GetSubKeyNames().Contains(subKeyName) 
                ? key.OpenSubKey(subKeyName)
                : key.CreateSubKey(subKeyName);
            if (subKeyDefault != null)
                subKey.SetValue(null, subKeyDefault);

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
    }
}