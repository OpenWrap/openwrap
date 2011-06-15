using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace OpenWrap.VisualStudio.Hooks
{
    public class PerUserComComponentInstaller
    {
        protected string _basePath;

        protected PerUserComComponentInstaller(string path, Type typeToRegister)
        {
            _basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), path);
            VersionProvider = file => StringExtensions.ToVersion(FileVersionInfo.GetVersionInfo(file).FileVersion);
            Type = typeToRegister;
            ProgId = Type.Attribute<ProgIdAttribute>().Value;
            Guid = Type.Attribute<GuidAttribute>().Value;

        }

        public const string ClrVersion2 = "v2.0.50727";
        public const string ClrVersion4 = "v4.0.30319";
        protected string Guid { get; set; }
        protected string ProgId { get; set; }
        protected Type Type { get; set; }
        public Func<string, Version> VersionProvider { get; set; }
        const string REG_INPROCSERVER32 = "InprocServer32";

        public void Uninstall()
        {
            if (Directory.Exists(_basePath))
            {
                try
                {
                    Directory.Delete(_basePath, true);
                }
                catch (IOException e)
                {
                }
            }
            var classes = Registry.CurrentUser.OpenSubKey("Software\\Classes", RegistryKeyPermissionCheck.ReadWriteSubTree);
            
            var clsIdKeyPath = Path.Combine("CLSID", "{" + Guid + "}");

            var clsIdKey = classes.OpenSubKey(clsIdKeyPath);
            if (clsIdKey != null)
            {
                clsIdKey.Close();
                classes.DeleteSubKeyTree(clsIdKeyPath);
            }
            var progIdKey = classes.OpenSubKey(ProgId);
            if (progIdKey != null)
            {
                progIdKey.Close();
                classes.DeleteSubKeyTree(ProgId);
            }
        }

        public void Install(string targetVersion)
        {
            string assemblyCodeBase = CopyAssemblyToAppData();
            if (assemblyCodeBase == null) return;
            var classes = Registry.CurrentUser.OpenSubKey("Software\\Classes", RegistryKeyPermissionCheck.ReadWriteSubTree);
            
            string className = Type.FullName;
            string assemblyName = Type.Assembly.GetName().FullName;

            string clsIdKey = Path.Combine("CLSID", "{" + Guid + "}");
            var rootComNode = classes.OpenSubKey(Combine(clsIdKey, REG_INPROCSERVER32));
            if (rootComNode != null)
            {
                var existingAssemblyName = rootComNode.GetValue("Assembly") as string;
                var existingClassName = rootComNode.GetValue("Class") as string;
                var existingPath = rootComNode.GetValue("CodeBase") as string;

                if (existingAssemblyName == assemblyName && existingClassName == className && existingPath == assemblyCodeBase)
                    return;
            }
            if (classes.OpenSubKey(ProgId) != null)
                classes.DeleteSubKeyTree(ProgId);
            if (classes.OpenSubKey(clsIdKey) != null)
                classes.DeleteSubKeyTree(clsIdKey);
            Debug.WriteLine("SolutionAddIn: Registering user hive COM object");
            classes
                // OpenWrap.VisualStudioIntegration
                .SubKey(ProgId,
                        ProgId,
                        x => x.SubKey("CLSID", "{" + Guid + "}")
                )
                .SubKey(clsIdKey,
                        ProgId,
                        x => x.SubKey(REG_INPROCSERVER32,
                                      "mscoree.dll",
                                      inproc =>
                                      inproc.Value("ThreadingModel", "Both")
                                          .Value("Class", className)
                                          .Value("Assembly", assemblyName)
                                          .Value("CodeBase", assemblyCodeBase)
                                          .Value("RuntimeVersion", targetVersion)
                                          .SubKey("1.0.0.0", version => version.Value("RuntimeVersion", targetVersion))
                                          .SubKey("ProgId", ProgId)
                                          .SubKey("Implemented Categories\\{62C8FE65-4EBB-45E7-B440-6E39B2CDBF29}")
                                 )
                );
        }

        static string Combine(params string[] strings)
        {
            if (strings.Length == 0) return string.Empty;
            return strings[0] + strings.Skip(1).Aggregate(string.Empty, (i, str) => i + "\\" + str);
        }

        string CopyAssemblyToAppData()
        {
            if (!Directory.Exists(_basePath))
                Directory.CreateDirectory(_basePath);

            var sourceAssemblyPath = Type.Assembly.Location;
            var sourceAssemblyFileName = Path.GetFileName(sourceAssemblyPath);

            if (sourceAssemblyFileName == null)
                return null;

            var currentVersion = VersionProvider(sourceAssemblyPath);

            var latestInstalledVersion = Directory.GetDirectories(_basePath)
                .Select(Path.GetFileName)
                .Where(x => x != null)
                .Select(_ => _.ToVersion())
                .Where(x => x != null)
                .OrderByDescending(x => x)
                .FirstOrDefault();

            if (latestInstalledVersion != null && latestInstalledVersion >= currentVersion) return null;

            var destinationPath = Path.Combine(_basePath, currentVersion.ToString());
            if (!Directory.Exists(destinationPath)) Directory.CreateDirectory(destinationPath);
            var destinationAssembly = Path.Combine(destinationPath, sourceAssemblyFileName);
            try
            {
                File.Copy(sourceAssemblyPath, destinationAssembly, true);
            }
            catch (Exception e)
            {
                Debug.WriteLine(string.Format("SolutionAddIn: Could not copy '{0}' to '{1}'.\r\n{2}", sourceAssemblyPath, destinationAssembly, e));
                return null;
            }
            return "file:///" + destinationAssembly.Replace('\\', '/');
        }

    }
}