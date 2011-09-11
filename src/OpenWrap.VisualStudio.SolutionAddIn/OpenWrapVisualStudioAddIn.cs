using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using EnvDTE;
using EnvDTE80;
using Extensibility;
using OpenWrap.Preloading;

namespace OpenWrap.VisualStudio.SolutionAddIn
{
    public abstract class OpenWrapVisualStudioAddIn : MarshalByRefObject, IDTExtensibility2
    {
        readonly string _progId;
        AppDomain _appDomain;
        IDisposable _appDomainManager;

        DTE _dte;
        OutputWindowPane _outputWindow;
        string _rootPath;

        public OpenWrapVisualStudioAddIn(string targetDteVersion, string progId)
        {
            _progId = progId;
            DteVersion = targetDteVersion;
        }

        public string DteVersion { get; private set; }

        protected AddIn AddIn { get; set; }
        protected DTE2 Application { get; set; }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public void OnAddInsUpdate(ref Array custom)
        {
        }

        public void OnBeginShutdown(ref Array custom)
        {
        }

        public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
        {
            var dte = application as DTE;
            if (dte == null) return;
            _dte = dte;
            try
            {
                if (dte.Version != DteVersion)
                {
                    Notify("OpenWrap Visual Studio integration is not correct version, re-creating now.");
                    dte.Solution.AddIns.OfType<AddIn>().First(x => x.ProgID == _progId).Remove();
                    if (dte.Version == "9.0")
                        dte.Solution.AddIns.Add(ComConstants.ADD_IN_PROGID_2008, ComConstants.ADD_IN_DESCRIPTION, ComConstants.ADD_IN_NAME, true);
                    else if (dte.Version == "10.0")
                        dte.Solution.AddIns.Add(ComConstants.ADD_IN_PROGID_2010, ComConstants.ADD_IN_DESCRIPTION, ComConstants.ADD_IN_NAME, true);
                    return;
                }
                _rootPath = GetRootLocation(dte.Solution.FullName);
                if (_rootPath == null) return;
                Notify("OpenWrap Visual Studio Integration ({0}) starting.", FileVersionInfo.GetVersionInfo(GetType().Assembly.Location).FileVersion);
                Notify("Root location: " + _rootPath);

                LoadAppDomain();
            }
            catch (Exception e)
            {
                Notify("OpenWrap failed to initialize. Improbability Drive has been activated.\r\n", e.ToString());
            }
        }

        public void OnDisconnection(ext_DisconnectMode removeMode, ref Array custom)
        {
            try
            {
                _appDomain.DomainUnload -= HandleAppDomainChange;
                _appDomainManager.Dispose();
            }
            catch
            {
            }
            Notify("Unloaded. Goodbye.");

            _appDomainManager = null;
            _dte = null;
            _appDomain = null;
        }

        public void OnStartupComplete(ref Array custom)
        {
        }

        static ResolveEventHandler HandleSelfLoad()
        {
            return (src, ev) =>
            {
                var currentAssembly = typeof(AddInAppDomainManager).Assembly;
                return (new AssemblyName(ev.Name).Name == currentAssembly.GetName().Name) ? currentAssembly : null;
            };
        }

        string GetRootLocation(string fullName)
        {
            string directoryName = Path.GetDirectoryName(fullName);
            if (directoryName == null) return null;

            var curDir = new DirectoryInfo(directoryName);
            do
            {
                if (curDir.GetFiles("*.wrapdesc").Length > 0) return curDir.FullName;
            } while ((curDir = curDir.Parent) != null);
            Notify("Could not locate descriptor file.");
            return null;
        }

        void LoadAppDomain()
        {
            Notify("Loading packages...");


            try
            {
                var packages = Preloader.GetPackageFolders(Preloader.RemoteInstall.None, _rootPath, null, "openwrap").ToArray();

                foreach (var package in packages) Notify("Loading package " + package);
                _appDomain = AppDomain.CreateDomain("OpenWrap Visual Studio Integration (default scope)");
                //_appDomain.SetupInformation = new AppDomainSetup() { ShadowCopyFiles = true }
                _appDomain.SetData("openwrap.vs.version", _dte.Version);
                _appDomain.SetData("openwrap.vs.currentdirectory", _rootPath);
                _appDomain.SetData("openwrap.vs.packages", packages.ToArray());
                _appDomain.SetData("openwrap.vs.appdomain", AppDomain.CurrentDomain);

                // replace that with the location in the codebase we just registered, ensuring the correct version is loaded
                var location = Type.GetTypeFromProgID(_progId).Assembly.Location;
                AppDomain.CurrentDomain.AssemblyResolve += HandleSelfLoad();
                _appDomainManager = (IDisposable)_appDomain.CreateInstanceFromAndUnwrap(location, typeof(AddInAppDomainManager).FullName);
                AppDomain.CurrentDomain.AssemblyResolve -= HandleSelfLoad();
                _appDomain.DomainUnload += HandleAppDomainChange;
            }
            catch (Exception e)
            {
                Notify("Could not load bootstrap packages.");
                Notify(e.ToString());
            }
        }

        void Notify(string message, params object[] args)
        {
            Debug.WriteLine(message);
            try
            {
                if (_outputWindow == null)
                {
                    var output = (OutputWindow)_dte.Windows.Item(Constants.vsWindowKindOutput).Object;

                    _outputWindow = output.OutputWindowPanes.Cast<OutputWindowPane>().FirstOrDefault(x => x.Name == "OpenWrap")
                                    ?? output.OutputWindowPanes.Add("OpenWrap");
                }

                _outputWindow.OutputString(string.Format(message, args) + "\r\n");
            }
            catch
            {
            }
        }

        void HandleAppDomainChange(object sender, EventArgs e)
        {
            try
            {
                Notify("Default scope change detected, restarting...");
                LoadAppDomain();
            }
            catch
            {
                Notify("Restarting failed, OpenWrap unloading.");
            }
        }
    }
}