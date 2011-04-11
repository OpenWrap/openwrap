
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.ComponentModel.Design;
//using System.Diagnostics;
//using System.Linq;
//using System.Reflection;
//using System.Runtime.CompilerServices;
//using System.Runtime.InteropServices;
//using System.Windows.Forms;
//using EnvDTE;
//using EnvDTE80;
//using Microsoft.VisualStudio;
//using Microsoft.VisualStudio.CommandBars;
//using Microsoft.VisualStudio.OLE.Interop;
//using Microsoft.VisualStudio.Shell;
//using Microsoft.VisualStudio.Shell.Interop;
//using mscoree;
//using OpenWrap.ComShim;
//using Constants = Microsoft.VisualStudio.Shell.Interop.Constants;
//using Debugger = System.Diagnostics.Debugger;
//using IServiceProvider = System.IServiceProvider;
//using IStream = Microsoft.VisualStudio.OLE.Interop.IStream;
//using Timer = System.Threading.Timer;

//namespace OpenWrap
//{
//    public class Test
//    {
//        static Timer _timer;

//        public static void Main()
//        {

//            AlertSolution();

//            //IVsOutputWindow outWindow = (IVsOutputWindow)Package.GetGlobalService(typeof(SVsOutputWindow));


//            //// Use e.g. Tools -> Create GUID to make a stable, but unique GUID for your pane. 

//            //Guid customGuid = new Guid("0F44E2D1-F5FA-4d2d-AB30-22BE8ECD9789");

//            //string customTitle = "OpenWrap";

//            //outWindow.CreatePane(ref customGuid, customTitle, 1, 1);



//            //IVsOutputWindowPane customPane;

//            //outWindow.GetPane(ref customGuid, out customPane);



//            //customPane.OutputString("Hello, Custom World!");

//            //customPane.Activate(); // Show the output window pane


//        }

//        static Guid OpenWrapPackageGuid = new Guid("{3A9801E8-4BAF-415B-A115-1F6D62967D24}");
//        static Guid OpenWrapCommandGroup = new Guid("{C11E43BD-0D8B-4816-AD3E-C626812FFAFE}");
        
//        static bool done;
//        static object sync = new object();

//        static void AlertSolution()
//        {
//            lock (sync)
//            {
//                if (done) return;
//                try
//                {
//                    var dte = Package.GetGlobalService(typeof(DTE)) as DTE
//                        ?? Package.GetGlobalService(typeof(SDTE)) as DTE
//                        ?? SiteManager.GetGlobalService<DTE>();
                
//                    var owner = (DTE2)dte;

//                    var menuService = CreateToolWindow(dte) ;
//                    var proffer = (IVsProfferCommands3)GetProffer(dte) ?? menuService.GetService<SVsProfferCommands, IVsProfferCommands3>();
                    
//                    var useless = dte.CommandBars;
//                    //CommandBar menuBarCommandBar = ((CommandBars)dte.CommandBars)["MenuBar"];


//                    object mainMenuAsObj;
//                    ErrorHandler.ThrowOnFailure(proffer.FindCommandBar(IntPtr.Zero, ref VsMenus.guidSHLMainMenu, 0, out mainMenuAsObj));

//                    var mainMenu = (CommandBar)mainMenuAsObj;

//                    var toolsMenu =
//                            mainMenu.Controls.OfType<CommandBarControl>()
//                                    .Where(x => x.Type == MsoControlType.msoControlPopup)
//                                    .Cast<CommandBarPopup>()
//                                    .Where(x => x.CommandBar.Name == "Tools")
//                                    .Select(x => x.CommandBar)
//                                    .First();

                    
//                    //, (uint)vsCommandControlType.vsCommandControlTypeButton


//                    uint openwrapCommandId;
//                    string pszCmdNameCanonical = "openwrap.4.canonical";
//                    try
//                    {
//                        proffer.RemoveNamedCommand(pszCmdNameCanonical);
//                    }
//                    catch
//                    {
//                    }
//                    //var parameters = new object[] { 
//                    //    OpenWrapPackageGuid, 
//                    //    OpenWrapCommandGroup, 
//                    //    "openwrap.1.canonical", 
//                    //    0u, 
//                    //    "openwrap.1.localized",
//                    //    "openwrap.1.text",
//                    //    "openwrap.1.tooltip",
//                    //    null, 
//                    //    0u,
//                    //    0u, 
//                    //    (uint)vsCommandDisabledFlags.vsCommandDisabledFlagsEnabled, 
//                    //    0u, 
//                    //    new Guid[0],
//                    //    (uint)vsCommandControlType.vsCommandControlTypeButton};

//                    //ErrorHandler.ThrowOnFailure((int)typeof(IVsProfferCommands3).GetMethod("AddNamedCommand2").Invoke(proffer, parameters));
//                    //openwrapCommandId = (uint)parameters[3];
//                    object pkgId = Guid.Empty;

//                    var contexts = new Guid[]{
//                        new Guid(ContextGuids.vsContextGuidSolutionExistsAndNotBuildingAndNotDebugging),
//                        new Guid(ContextGuids.vsContextGuidSolutionHasMultipleProjects),
//                        new Guid(ContextGuids.vsContextGuidSolutionHasSingleProject),
//                        new Guid(ContextGuids.vsContextGuidSolutionExplorer),
//                        new Guid(ContextGuids.vsContextGuidDesignMode),
//                        new Guid(ContextGuids.vsContextGuidFrames),
//                        new Guid(ContextGuids.vsContextGuidLinkedWindowFrame),
//                        new Guid(ContextGuids.vsContextGuidMainWindow),
//                        new Guid(ContextGuids.vsContextGuidNotBuildingAndNotDebugging),
//                        new Guid(ContextGuids.vsContextGuidApplicationBrowser),
//                    new Guid(ContextGuids.vsContextGuidSolutionExists)
//                    };
//                    ErrorHandler.ThrowOnFailure(proffer.AddNamedCommand(
//                            ref OpenWrapPackageGuid,
//                            ref OpenWrapCommandGroup,
//                            pszCmdNameCanonical,
//                            out openwrapCommandId,
//                            "openwrap.4.localized",
//                            "openwrap.4.text",
//                            "openwrap.4.tooltip",
//                            null,
//                            0u,
//                            0u,
//                            (uint)vsCommandDisabledFlags.vsCommandDisabledFlagsEnabled,
//                            (uint)contexts.Length,
//                            contexts));

//                    var c2 = dte.Commands as Commands2;
                    
//                    string commandCanonicalName = pszCmdNameCanonical;
//                    Command testCommand = GetCommandByName(c2, commandCanonicalName);

//                    if (testCommand == null)
//                        throw new InvalidOperationException();
                    

//                    Console.WriteLine(testCommand.IsAvailable);
//                    if (toolsMenu.Controls.Count > 0)
//                    {
//                        List<CommandBarControl> controlsToDelete = new List<CommandBarControl>();
//                        foreach (CommandBarControl control in toolsMenu.Controls)
//                        {

//                            if (control == null) continue;
//                            if (control.Caption.StartsWith("openwrap.1."))
//                            {
//                                controlsToDelete.Add(control);
//                            }
//                        }
//                        foreach (var control in controlsToDelete)
//                            control.Delete();
//                    }

//                    var uiControl = (CommandBarControl)testCommand.AddControl(toolsMenu, 1);
//                    //uiControl.Enabled = true;

//                    var priority = ((IServiceProvider)menuService).GetService<SVsRegisterPriorityCommandTarget, IVsRegisterPriorityCommandTarget>();
//                    var blah = new testHandler();
//                    uint pdwCookie;
//                    ErrorHandler.ThrowOnFailure(priority.RegisterPriorityCommandTarget(0u, blah, out pdwCookie));
//                    ////var newlyAddedCommand = new CommandID(OpenWrapCommandGroup, (int)openwrapCommandId);

//                    ////EventHandler ev = (s, e) => MessageBox.Show("Weepee");

//                    ////var oleCommand = new OleMenuCommand(ev,
//                    ////                                    (s, e) =>
//                    ////                                    {
//                    ////                                        Console.WriteLine("Change");
//                    ////                                    },
//                    ////                                    (s, e) =>
//                    ////                                    {
//                    ////                                        Console.WriteLine("Change");
//                    ////                                    },
//                    ////                                    newlyAddedCommand, "text");


//                    ////menuService.GetService<IMenuCommandService,OleMenuCommandService>().AddCommand(oleCommand);
//                    ////oleCommand.Enabled = true;
//                    ////menuService.GetService<IVsUIShell>().UpdateCommandUI(0);
//                    done = true;
//                }
//                catch (Exception e)
//                {
//                    Debug.WriteLine(e.ToString());
//                }
//            }
//        }
//        class testHandler : IOleCommandTarget
//        {
            
//            public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
//            {
//                if (pguidCmdGroup == OpenWrapCommandGroup)
//                {
//                    Console.WriteLine("Called");
//                    prgCmds[0].cmdf = (uint)NativeMethods.tagOLECMDF.OLECMDF_ENABLED;
//                    prgCmds[0].cmdf |= (uint)NativeMethods.tagOLECMDF.OLECMDF_SUPPORTED;
//                    return NativeMethods.S_OK;
//                }
//                return NativeMethods.OLECMDERR_E_NOTSUPPORTED;
//            }

//            public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
//            {
                
//            }
//        }
//        static AddIn _tempAddin = new tempAddin();
//        static Window _window;

//        static IWindowPaneBehavior CreateToolWindow(DTE owner)
//        {
//            //owner.Solution.AddIns.Add(_tempAddin);
//            //var win = owner.Windows as Windows2;
//            //object something = null;
//            //_window = win.CreateToolWindow2(_tempAddin, typeof(Test).Assembly.FullName, typeof(Control).FullName, "Caption", Control.ToolWindowID.ToString("B"), something);
//            //owner.Events.DTEEvents.OnStartupComplete += () => _window.Visible = true;
//            //owner.Events.DTEEvents.ModeChanged += mode => Console.WriteLine(mode);
//            //IVsUIShell s;
            
//            //_window.Visible = true;
//            //new Timer(_=> _window.Visible = true, null, TimeSpan.FromSeconds(15), TimeSpan.FromMilliseconds(-1));
//            IWindowPaneBehavior pane = new WindowPane<Control>(new ServiceProvider((Microsoft.VisualStudio.OLE.Interop.IServiceProvider)owner));
//            CreateToolWindow(pane, 0);

//            return pane;


//        }

//        private static void CreateToolWindow(IWindowPaneBehavior toolWindowType, int id)
//        {
//            if (id < 0)
//                throw new ArgumentOutOfRangeException();

//            // --- First create an instance of the ToolWindowPane
//            var window = toolWindowType;
//            //window.SetInstanceId(id);

//            // --- Check if this window has a ToolBar
//            bool hasToolBar = false;

//            var flags = (uint)__VSCREATETOOLWIN.CTW_fInitNew;
//            //if (!tool.Transient)
//            //    flags |= (uint)__VSCREATETOOLWIN.CTW_fForceCreate;
//            //if (hasToolBar)
//            //    flags |= (uint)__VSCREATETOOLWIN.CTW_fToolbarHost;
//            //if (tool.MultiInstances)
//            //    flags |= (uint)__VSCREATETOOLWIN.CTW_fMultiInstance;
//            Guid emptyGuid = Guid.Empty;
//            Guid toolClsid = Guid.Empty;
//            IVsWindowPane windowPane = null;
//            if (toolClsid.CompareTo(Guid.Empty) == 0)
//            {
//                // --- If a tool CLSID is not specified, then host the IVsWindowPane
//                windowPane = window;
//            }
//            Guid persistenceGuid = Control.ToolWindowID;
//            IVsWindowFrame windowFrame;
//            // Use IVsUIShell to create frame.
//            var vsUiShell = SiteManager.GetGlobalService<SVsUIShell, IVsUIShell>();
//            if (vsUiShell == null)
//                throw new Exception();

//            int hr = vsUiShell.CreateToolWindow(flags, // flags
//                                                (uint)id, // instance ID
//                                                windowPane,
//                // IVsWindowPane to host in the toolwindow (null if toolClsid is specified)
//                                                ref toolClsid,
//                // toolClsid to host in the toolwindow (Guid.Empty if windowPane is not null)
//                                                ref persistenceGuid, // persistence Guid
//                                                ref emptyGuid, // auto activate Guid
//                                                null, // service provider
//                                                "Caption title", // Window title
//                                                null,
//                                                out windowFrame);
//            NativeMethods.ThrowOnFailure(hr);

//            // --- If the toolwindow is a component, site it.
//            IComponent component = null;
//            if (window.Window is IComponent)
//                component = (IComponent)window.Window;
//            else if (windowPane is IComponent)
//                component = (IComponent)windowPane;

//            // --- This generates the OnToolWindowCreated event on the ToolWindowPane
//            window.Frame = new WindowFrame(windowFrame);

//            //if (hasToolBar && windowFrame != null)
//            //{
//            //    // --- Set the toolbar
//            //    object obj;
//            //    NativeMethods.ThrowOnFailure(windowFrame.GetProperty(
//            //                                   (int)__VSFPROPID.VSFPROPID_ToolbarHost, out obj));
//            //    var toolBarHost = obj as IVsToolWindowToolbarHost;
//            //    if (toolBarHost != null)
//            //    {
//            //        Guid toolBarCommandSet = window.ToolBar.Guid;
//            //        NativeMethods.ThrowOnFailure(
//            //          toolBarHost.AddToolbar((VSTWT_LOCATION)window.ToolBarLocation, ref toolBarCommandSet,
//            //                                 (uint)window.ToolBar.ID));
//            //        window.OnToolBarAdded();
//            //    }
//            //}

//            //// --- If the ToolWindow was created successfully, keep track of it
//            //VsDebug.Assert(window != null, "At this point window assumed to be non-null.");
//            //Dictionary<int, IToolWindowPaneBehavior> toolInstances;
//            //if (!_ToolWindows.TryGetValue(toolWindowType, out toolInstances))
//            //{
//            //    toolInstances = new Dictionary<int, IToolWindowPaneBehavior>();
//            //    _ToolWindows.Add(toolWindowType, toolInstances);
//            //}
//            //VsDebug.Assert(!toolInstances.ContainsKey(id),
//            //               "An existing tool window instance has been recreated.");
//            //toolInstances.Add(id, window);
//            //return window;
//        }
//        public class Control : UserControl
//        {
//            public static Guid ToolWindowID = new Guid("{35550B76-5D8A-49F7-AE4F-1F883B2DF3A7}");
//            public Control()
//            {
//                this.Controls.Add(new TextBox { Text = (this.GetService(typeof(IOleCommandTarget)) == null).ToString() });
//            }
//        }
//        class tempAddin : AddIn{
//            public void Remove()
//            {
                
//            }

//            public string Description { get; set; }

//            public AddIns Collection { get; set; }

//            public string ProgID
//            {
//                get { return "openwrap.addin"; }
//            }

//            static Guid addinId = new Guid("{9EEC6DCF-E85B-479E-868B-FFF195289B66}");
//            public string Guid
//            {
//                get { return addinId.ToString(); }
//            }

//            public bool Connected { get; set; }

//            public object Object { get; set; }

//            public DTE DTE { get; set; }

//            public string Name
//            {
//                get { return "Test"; }

//            }

//            public string SatelliteDllPath
//            {
//                get { return null; }
//            }
//        }
//        static Command GetCommandByName(Commands2 c2, string commandCanonicalName)
//        {
//        }

//        static object GetProffer(DTE dte)
//        {
//            //var oleSite = (IOleServiceProvider)
//            return Package.GetGlobalService(typeof(SVsProfferCommands));
//        }

//        static void control_Click(CommandBarButton Ctrl, ref bool CancelDefault)
//        {
//            MessageBox.Show("Clicked, waddayaknow");
//        }


//        //public static TService Get<TConcrete, TService>()
//        //{
//        //    object blah = Package.GetGlobalService(typeof(TConcrete));
//        //    return (TService)blah;
//        //}
//    }

//    [ComImport, InterfaceType((short)1), Guid("3A83904D-4540-4C51-95A7-618B32A9A9C0")]
//    public interface IVsProfferCommands3
//    {
//        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
//        int AddNamedCommand([In] ref Guid pguidPackage, [In] ref Guid pguidCmdGroup, [In, ComAliasName("Microsoft.VisualStudio.OLE.Interop.LPCOLESTR"), MarshalAs(UnmanagedType.LPWStr)] string pszCmdNameCanonical, [ComAliasName("Microsoft.VisualStudio.OLE.Interop.DWORD")] out uint pdwCmdId, [In, ComAliasName("Microsoft.VisualStudio.OLE.Interop.LPCOLESTR"), MarshalAs(UnmanagedType.LPWStr)] string pszCmdNameLocalized, [In, ComAliasName("Microsoft.VisualStudio.OLE.Interop.LPCOLESTR"), MarshalAs(UnmanagedType.LPWStr)] string pszBtnText, [In, ComAliasName("Microsoft.VisualStudio.OLE.Interop.LPCOLESTR"), MarshalAs(UnmanagedType.LPWStr)] string pszCmdTooltip, [In, ComAliasName("Microsoft.VisualStudio.OLE.Interop.LPCOLESTR"), MarshalAs(UnmanagedType.LPWStr)] string pszSatelliteDLL, [In, ComAliasName("Microsoft.VisualStudio.OLE.Interop.DWORD")] uint dwBitmapResourceId, [In, ComAliasName("Microsoft.VisualStudio.OLE.Interop.DWORD")] uint dwBitmapImageIndex, [In, ComAliasName("Microsoft.VisualStudio.OLE.Interop.DWORD")] uint dwCmdFlagsDefault, [In, ComAliasName("Microsoft.VisualStudio.OLE.Interop.DWORD")] uint cUIContexts, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 11)] Guid[] rgguidUIContexts);
//        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
//        int RemoveNamedCommand([In, ComAliasName("Microsoft.VisualStudio.OLE.Interop.LPCOLESTR"), MarshalAs(UnmanagedType.LPWStr)] string pszCmdNameCanonical);
//        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
//        int RenameNamedCommand([In, ComAliasName("Microsoft.VisualStudio.OLE.Interop.LPCOLESTR"), MarshalAs(UnmanagedType.LPWStr)] string pszCmdNameCanonical, [In, ComAliasName("Microsoft.VisualStudio.OLE.Interop.LPCOLESTR"), MarshalAs(UnmanagedType.LPWStr)] string pszCmdNameCanonicalNew, [In, ComAliasName("Microsoft.VisualStudio.OLE.Interop.LPCOLESTR"), MarshalAs(UnmanagedType.LPWStr)] string pszCmdNameLocalizedNew);
//        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
//        int AddCommandBarControl([In, ComAliasName("Microsoft.VisualStudio.OLE.Interop.LPCOLESTR"), MarshalAs(UnmanagedType.LPWStr)] string pszCmdNameCanonical, [In, MarshalAs(UnmanagedType.IDispatch)] object pCmdBarParent, [In, ComAliasName("Microsoft.VisualStudio.OLE.Interop.DWORD")] uint dwIndex, [In, ComAliasName("Microsoft.VisualStudio.OLE.Interop.DWORD")] uint dwCmdType, [MarshalAs(UnmanagedType.IDispatch)] out object ppCmdBarCtrl);
//        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
//        int RemoveCommandBarControl([In, MarshalAs(UnmanagedType.IDispatch)] object pCmdBarCtrl);
//        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
//        int AddCommandBar([In, ComAliasName("Microsoft.VisualStudio.OLE.Interop.LPCOLESTR"), MarshalAs(UnmanagedType.LPWStr)] string pszCmdBarName, [In, ComAliasName("Microsoft.VisualStudio.OLE.Interop.DWORD")] uint dwType, [In, MarshalAs(UnmanagedType.IDispatch)] object pCmdBarParent, [In, ComAliasName("Microsoft.VisualStudio.OLE.Interop.DWORD")] uint dwIndex, [MarshalAs(UnmanagedType.IDispatch)] out object ppCmdBar);
//        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
//        int RemoveCommandBar([In, MarshalAs(UnmanagedType.IDispatch)] object pCmdBar);
//        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
//        int FindCommandBar([In, MarshalAs(UnmanagedType.IUnknown)] object pToolbarSet, [In] ref Guid pguidCmdGroup, [In, ComAliasName("Microsoft.VisualStudio.OLE.Interop.DWORD")] uint dwMenuId, [MarshalAs(UnmanagedType.IDispatch)] out object ppdispCmdBar);
//        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
//        int AddNamedCommand2([In] ref Guid pguidPackage, [In] ref Guid pguidCmdGroup, [In, ComAliasName("Microsoft.VisualStudio.OLE.Interop.LPCOLESTR"), MarshalAs(UnmanagedType.LPWStr)] string pszCmdNameCanonical, [ComAliasName("Microsoft.VisualStudio.OLE.Interop.DWORD")] out uint pdwCmdId, [In, ComAliasName("Microsoft.VisualStudio.OLE.Interop.LPCOLESTR"), MarshalAs(UnmanagedType.LPWStr)] string pszCmdNameLocalized, [In, ComAliasName("Microsoft.VisualStudio.OLE.Interop.LPCOLESTR"), MarshalAs(UnmanagedType.LPWStr)] string pszBtnText, [In, ComAliasName("Microsoft.VisualStudio.OLE.Interop.LPCOLESTR"), MarshalAs(UnmanagedType.LPWStr)] string pszCmdTooltip, [In, ComAliasName("Microsoft.VisualStudio.OLE.Interop.LPCOLESTR"), MarshalAs(UnmanagedType.LPWStr)] string pszSatelliteDLL, [In, ComAliasName("Microsoft.VisualStudio.OLE.Interop.DWORD")] uint dwBitmapResourceId, [In, ComAliasName("Microsoft.VisualStudio.OLE.Interop.DWORD")] uint dwBitmapImageIndex, [In, ComAliasName("Microsoft.VisualStudio.OLE.Interop.DWORD")] uint dwCmdFlagsDefault, [In, ComAliasName("Microsoft.VisualStudio.OLE.Interop.DWORD")] uint cUIContexts, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 11)] Guid[] rgguidUIContexts, [In, ComAliasName("Microsoft.VisualStudio.OLE.Interop.DWORD")] uint dwUIElementType);
//    }

 

 


//    [
//  ComImport,
//  Guid("6D5140C1-7436-11CE-8034-00AA006009FA"),
//  InterfaceTypeAttribute(ComInterfaceType.
//                         InterfaceIsIUnknown)
//]
//    public interface ICustomOleServiceProvider
//    {
//        [PreserveSig]
//        int QueryService([In]ref Guid guidService,
//           [In]ref Guid riid,
//           [MarshalAs(UnmanagedType.Interface)] out 
//      System.Object obj);
//    }
//}