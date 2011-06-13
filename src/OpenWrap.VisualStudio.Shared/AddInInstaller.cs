using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.Win32;
using OpenWrap.VisualStudio.SolutionAddIn;

namespace OpenWrap.VisualStudio.Hooks
{
    public static class AddInInstaller
    {
        const string REG_INPROCSERVER32 = "InprocServer32";

        public static void InstallInUserHive()
        {
            var assemblyCodeBase = CopyAssemblyToAppData();
            if (assemblyCodeBase == null) return;
            RegisterComAddIn<OpenWrapVisualStudioAddIn2008>(assemblyCodeBase, "v2.0.50727");
            RegisterComAddIn<OpenWrapVisualStudioAddIn2010>(assemblyCodeBase, "v4.0.30319");
        }
        public static void Uninstall()
        {
            var localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var openwrapPath = Path.Combine(localAppDataPath, "openwrap");
            var addinPath = Path.Combine(openwrapPath, "VisualStudio");

            if (Directory.Exists(addinPath))
                Directory.Delete(addinPath, true);

            UnregisterComAddIn<OpenWrapVisualStudioAddIn2008>();
            UnregisterComAddIn<OpenWrapVisualStudioAddIn2010>();
        }

        static void UnregisterComAddIn<T>()
        {
            var classes = Registry.CurrentUser.OpenSubKey("Software\\Classes", RegistryKeyPermissionCheck.ReadWriteSubTree);
            if (classes == null) return;

            var type = typeof(T);
            var progid = type.Attribute<ProgIdAttribute>().Value;
            var guid = type.Attribute<GuidAttribute>().Value;
            
            string clsIdKeyPath = Path.Combine("CLSID", "{" + guid + "}");

            var clsIdKey = classes.OpenSubKey(clsIdKeyPath);
            if (clsIdKey != null)
            {
                clsIdKey.Close();
                classes.DeleteSubKeyTree(clsIdKeyPath);
            }
            var progIdKey = classes.OpenSubKey(progid);
            if (progIdKey != null)
            {
                progIdKey.Close();
                classes.DeleteSubKeyTree(progid);
            }
        }

        static void RegisterComAddIn<T>(string assemblyCodeBase, string targetVersion)
        {
            var type = typeof(T);
            var progid = type.Attribute<ProgIdAttribute>().Value;
            var guid = type.Attribute<GuidAttribute>().Value;


            var registeredType = Type.GetTypeFromProgID(progid, null, false);
            if (registeredType != null) return;

            var classes = Registry.CurrentUser.OpenSubKey("Software\\Classes", RegistryKeyPermissionCheck.ReadWriteSubTree);
            if (classes == null) return;

            string className = type.FullName;
            string assemblyName = type.Assembly.GetName().FullName;

            string clsIdKey = Path.Combine("CLSID", "{" + guid + "}");
            var rootComNode = classes.OpenSubKey(Combine(clsIdKey, REG_INPROCSERVER32));
            if (rootComNode != null)
            {
                var existingAssemblyName = rootComNode.GetValue("Assembly") as string;
                var existingClassName = rootComNode.GetValue("Class") as string;
                var existingPath = rootComNode.GetValue("CodeBase") as string;

                if (existingAssemblyName == assemblyName && existingClassName == className && existingPath == assemblyCodeBase)
                    return;
            }
            if (classes.OpenSubKey(progid) != null)
                classes.DeleteSubKeyTree(progid);
            if (classes.OpenSubKey(clsIdKey) != null)
                classes.DeleteSubKeyTree(clsIdKey);
            Debug.WriteLine("SolutionAddIn: Registering user hive COM object");
            classes
                // OpenWrap.VisualStudioIntegration
                .SubKey(progid,
                        progid,
                        x => x.SubKey("CLSID", "{" + guid + "}")
                )
                .SubKey(clsIdKey,
                        progid,
                        x => x.SubKey(REG_INPROCSERVER32,
                                      "mscoree.dll",
                                      inproc =>
                                      inproc.Value("ThreadingModel", "Both")
                                          .Value("Class", className)
                                          .Value("Assembly", assemblyName)
                                          .Value("CodeBase", assemblyCodeBase)
                                          .Value("RuntimeVersion", targetVersion)
                                          .SubKey("1.0.0.0",version => version.Value("RuntimeVersion", targetVersion))
                                          .SubKey("ProgId", progid)
                                          .SubKey("Implemented Categories\\{62C8FE65-4EBB-45E7-B440-6E39B2CDBF29}")
                                 )
                );
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
            var vsIntegrationPath = Path.Combine(openwrapPath, "VisualStudio");

            if (!Directory.Exists(openwrapPath))
                Directory.CreateDirectory(openwrapPath);
            if (!Directory.Exists(vsIntegrationPath))
                Directory.CreateDirectory(vsIntegrationPath);

            var sourceAssembly = typeof(OpenWrapVisualStudioAddIn).Assembly.Location;
            var sourceAssemblyFileName = Path.GetFileName(sourceAssembly);

            if (sourceAssemblyFileName == null)
                return null;

            var currentVersion = FileVersionInfo.GetVersionInfo(sourceAssembly).FileVersion.ToVersion();

            var latestInstalledVersion = Directory.GetDirectories(vsIntegrationPath)
                .Select(Path.GetDirectoryName)
                .Where(x => x != null)
                .Select(_ => _.ToVersion())
                .Where(x => x != null)
                .OrderByDescending(x => x)
                .FirstOrDefault();

            if (latestInstalledVersion != null && latestInstalledVersion > currentVersion) return null;

            var destinationPath = Path.Combine(vsIntegrationPath, currentVersion.ToString());
            if (!Directory.Exists(destinationPath)) Directory.CreateDirectory(destinationPath);
            var destinationAssembly = Path.Combine(destinationPath, sourceAssemblyFileName);
            try
            {
                File.Copy(sourceAssembly, destinationAssembly, true);
            }
            catch (Exception e)
            {
                Debug.WriteLine(string.Format("SolutionAddIn: Could not copy '{0}' to '{1}'.\r\n{2}", sourceAssembly, destinationAssembly, e));
                return null;
            }
            return "file:///" + destinationAssembly.Replace('\\', '/');
        }
    }
}