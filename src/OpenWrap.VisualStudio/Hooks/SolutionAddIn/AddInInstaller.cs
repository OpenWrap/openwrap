using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using OpenWrap.VisualStudio.ComShim;

namespace OpenWrap.VisualStudio.Hooks
{
    public static class AddInInstaller
    {
        const string REG_INPROCSERVER32 = "InprocServer32";

        public static bool RegisterAddInInUserHive()
        {
            var assemblyCodeBase = CopyAssemblyToAppData();
            var registeredType = Type.GetTypeFromProgID(Constants.ADD_IN_PROGID, null, false);
            if (registeredType != null) return true;

            var classes = Registry.CurrentUser.OpenSubKey("Software\\Classes", RegistryKeyPermissionCheck.ReadWriteSubTree);
            if (classes == null) return false;

            string className = typeof(OpenWrapBootstrapper).FullName;
            string assemblyName = typeof(OpenWrapBootstrapper).Assembly.GetName().FullName;

            string clsIdKey = Path.Combine("CLSID", "{" + Constants.ADD_IN_GUID + "}");
            var rootComNode = classes.OpenSubKey(Combine(clsIdKey, REG_INPROCSERVER32));
            if (rootComNode != null)
            {
                var existingAssemblyName = rootComNode.GetValue("Assembly") as string;
                var existingClassName = rootComNode.GetValue("Class") as string;
                var existingPath = rootComNode.GetValue("CodeBase") as string;

                if (existingAssemblyName == assemblyName && existingClassName == className && existingPath == assemblyCodeBase)
                    return true;
            }
            if (classes.OpenSubKey(Constants.ADD_IN_PROGID) != null)
                classes.DeleteSubKeyTree(Constants.ADD_IN_PROGID);
            if (classes.OpenSubKey(clsIdKey) != null)
                classes.DeleteSubKeyTree(clsIdKey);
            Debug.WriteLine("SolutionAddIn: Registering user hive COM object");
            classes
                    .SubKey(Constants.ADD_IN_PROGID,
                            Constants.ADD_IN_PROGID,
                            x =>
                            x.SubKey(clsIdKey)
                    )
                    .SubKey(clsIdKey,
                            Constants.ADD_IN_PROGID,
                            x =>
                            x.SubKey(REG_INPROCSERVER32,
                                     "mscoree.dll",
                                     inproc =>
                                     inproc.Value("ThreadingModel", "Both")
                                             .Value("Class", className)
                                             .Value("Assembly", assemblyName)
                                             .Value("CodeBase", assemblyCodeBase)
                                             .Value("RuntimeVersion", "v2.0.50727")
                                             .SubKey("1.0.0.0",
                                                     version =>
                                                     version.Value("RuntimeVersion", "v2.0.50727")
                                             )
                                             .SubKey("ProgId", Constants.ADD_IN_PROGID)
                                             .SubKey("Implemented Categories\\{62C8FE65-4EBB-45E7-B440-6E39B2CDBF29}")
                                    )
                    );

            return true;
        }

        static string Combine(params string[] strings)
        {
            if (strings.Length == 0) return string.Empty;
            return strings[0] + strings.Skip(1).Aggregate(string.Empty, (i, str) => i + "\\" + str);
        }

        static string CopyAssemblyToAppData()
        {
            var localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var openwrapPath = Path.Combine(localAppDataPath, "openwrap");
            var addinPath = Path.Combine(openwrapPath, "VisualStudio");

            if (!Directory.Exists(openwrapPath))
                Directory.CreateDirectory(openwrapPath);
            if (!Directory.Exists(addinPath))
                Directory.CreateDirectory(addinPath);

            var existingPath = typeof(OpenWrapBootstrapper).Assembly.Location;
            var assemblyFileName = Path.GetFileName(existingPath);
            if (assemblyFileName == null)
                return null;
            var assemblyPath = Path.Combine(addinPath, assemblyFileName);
            try
            {
                File.Copy(existingPath, assemblyPath, true);
            }
            catch (Exception e)
            {
                Debug.WriteLine(string.Format("SolutionAddIn: Could not copy '{0}' to '{1}'.\r\n{2}", existingPath, assemblyPath, e));
                return null;
            }
            return "file:///" + assemblyPath.Replace('\\', '/');
        }
    }
}