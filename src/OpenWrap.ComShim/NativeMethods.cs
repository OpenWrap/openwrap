using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using IServiceProvider = System.IServiceProvider;

namespace OpenWrap.ComShim
{
    /// <summary>
    /// Static class for native P/Invoke methods and types
    /// </summary>
    // ================================================================================================
    public static class NativeMethods
    {
        #region ExtendedSpecialFolder enum

        /// <summary>
        /// Defines the values that are not supported by the System.Environment.SpecialFolder enumeration
        /// </summary>
        [ComVisible(true)]
        public enum ExtendedSpecialFolder
        {
            /// <summary>
            /// Identical to CSIDL_COMMON_STARTUP
            /// </summary>
            CommonStartup = 0x0018,

            /// <summary>
            /// Identical to CSIDL_WINDOWS 
            /// </summary>
            Windows = 0x0024,
        }

        #endregion

        #region PARAMETER_PASSING_MODE enum

        public enum PARAMETER_PASSING_MODE
        {
            cmParameterTypeIn = 1,
            cmParameterTypeOut = 2,
            cmParameterTypeInOut = 3
        }

        #endregion

        #region tagOLECMDF enum

        /// <devdoc>
        /// OLECMDF enums for IOleCommandTarget
        /// </devdoc>
        public enum tagOLECMDF
        {
            OLECMDF_SUPPORTED = 1,
            OLECMDF_ENABLED = 2,
            OLECMDF_LATCHED = 4,
            OLECMDF_NINCHED = 8,
            OLECMDF_INVISIBLE = 16
        }

        #endregion

        #region VSTASKBITMAP enum

        /// <summary>
        /// Specifies options for a bitmap image associated with a task item.
        /// </summary>
        public enum VSTASKBITMAP
        {
            BMP_COMPILE = -1,
            BMP_SQUIGGLE = -2,
            BMP_COMMENT = -3,
            BMP_SHORTCUT = -4,
            BMP_USER = -5
        } ;

        #endregion

        #region SHFILEINFO Struct

        /// <summary>
        /// SHFILEINFO Struct
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct SHFILEINFO
        {
            /// <summary>
            /// 
            /// </summary>
            public IntPtr hIcon;
            /// <summary>
            /// 
            /// </summary>
            public IntPtr iIcon;
            /// <summary>
            /// 
            /// </summary>
            public uint dwAttributes;
            /// <summary>
            /// 
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            /// <summary>
            /// 
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        public const uint SHGFI_ICON = 0x100;
        public const uint SHGFI_LARGEICON = 0x0;
        public const uint SHGFI_SMALLICON = 0x1;

        #endregion

        public const ushort CF_HDROP = 15; // winuser.h
        public const int DWLP_MSGRESULT = 0;

        public const int E_ABORT = -2147467260;
        public const int E_ACCESSDENIED = -2147024891;
        public const int E_FAIL = -2147467259;
        public const int E_HANDLE = -2147024890;
        public const int E_INVALIDARG = -2147024809;
        public const int E_NOINTERFACE = -2147467262;
        public const int E_NOTIMPL = -2147467263;
        public const int E_OUTOFMEMORY = -2147024882;
        public const int E_PENDING = -2147483638;
        public const int E_POINTER = -2147467261;
        public const int E_UNEXPECTED = -2147418113;

        public const int
          FILE_ATTRIBUTE_READONLY = 0x00000001;

        public const int
          FW_BOLD = 700;

        public const int
          GMEM_DDESHARE = 0x2000;

        public const int
          GMEM_MOVEABLE = 0x0002,
          GMEM_ZEROINIT = 0x0040;

        public const int DWL_MSGRESULT = 0;
        public const int GWL_EXSTYLE = -20;
        public const int GWL_STYLE = -16;
        public const int GWL_WNDPROC = -4;
        public const int ILD_MASK = 0x0010;
        public const int ILD_NORMAL = 0x0000;
        public const int ILD_ROP = 0x0040;
        public const int ILD_TRANSPARENT = 0x0001;
        public const int MAX_PATH = 260; // windef.h	
        public const uint MK_CONTROL = 0x0008; //winuser.h
        public const uint MK_SHIFT = 0x0004;
        public const int OLECMDERR_E_NOTSUPPORTED = -2147221248;
        public const int OLECMDERR_E_UNKNOWNGROUP = -2147221244;

        public const int OPAQUE = 2;

        public const int
          PSBTN_APPLYNOW = 4;

        public const int
          PSBTN_BACK = 0;

        public const int
          PSBTN_CANCEL = 5;

        public const int
          PSBTN_FINISH = 2;

        public const int
          PSBTN_HELP = 6,
          PSBTN_MAX = 6;

        public const int
          PSBTN_NEXT = 1;

        public const int
          PSBTN_OK = 3;

        public const int
          PSH_DEFAULT = 0x00000000;

        public const int
          PSH_HASHELP = 0x00000200;

        public const int
          PSH_HEADER = 0x00080000;

        public const int
          PSH_MODELESS = 0x00000400;

        public const int
          PSH_NOAPPLYNOW = 0x00000080;

        public const int
          PSH_NOCONTEXTHELP = 0x02000000;

        public const int
          PSH_PROPSHEETPAGE = 0x00000008;

        public const int
          PSH_PROPTITLE = 0x00000001;

        public const int
          PSH_RTLREADING = 0x00000800;

        public const int
          PSH_STRETCHWATERMARK = 0x00040000;

        public const int
          PSH_USECALLBACK = 0x00000100;

        public const int
          PSH_USEHBMHEADER = 0x00100000;

        public const int
          PSH_USEHBMWATERMARK = 0x00010000;

        public const int
          PSH_USEHICON = 0x00000002;

        public const int
            // user pass in a hbmWatermark instead of pszbmWatermark
          PSH_USEHPLWATERMARK = 0x00020000;

        public const int
          PSH_USEICONID = 0x00000004;

        public const int
          PSH_USEPAGELANG = 0x00200000;

        public const int
          PSH_USEPSTARTPAGE = 0x00000040;

        public const int
          PSH_WATERMARK = 0x00008000;

        public const int
          PSH_WIZARD = 0x00000020;

        public const int
            // use frame dialog template matched to page
          PSH_WIZARD_LITE = 0x00400000;

        public const int
          PSH_WIZARDCONTEXTHELP = 0x00001000;

        public const int
          PSH_WIZARDHASFINISH = 0x00000010;

        public const int
          PSN_APPLY = ((0 - 200) - 2),
          PSN_KILLACTIVE = ((0 - 200) - 1),
          PSN_RESET = ((0 - 200) - 3),
          PSN_SETACTIVE = ((0 - 200) - 0);

        public const int PSNRET_INVALID = 1,
                         PSNRET_INVALID_NOCHANGEPAGE = 2;

        public const int PSNRET_NOERROR = 0;

        public const int
          PSP_DEFAULT = 0x00000000,
          PSP_DLGINDIRECT = 0x00000001;

        public const int
          PSP_HASHELP = 0x00000020;

        public const int
          PSP_HIDEHEADER = 0x00000800;

        public const int
          PSP_PREMATURE = 0x00000400;

        public const int
          PSP_RTLREADING = 0x00000010;

        public const int
          PSP_USECALLBACK = 0x00000080;

        public const int
          PSP_USEHEADERSUBTITLE = 0x00002000;

        public const int
          PSP_USEHEADERTITLE = 0x00001000;

        public const int
          PSP_USEHICON = 0x00000002,
          PSP_USEICONID = 0x00000004;

        public const int
          PSP_USEREFPARENT = 0x00000040;

        public const int
          PSP_USETITLE = 0x00000008;

        public const int S_FALSE = 1;
        public const int S_OK = 0;
        public const int SW_SHOWNORMAL = 1;

        public const int
          SWP_FRAMECHANGED = 0x0020;

        public const int
          SWP_NOACTIVATE = 0x0010;

        public const int
          SWP_NOMOVE = 0x0002;

        public const int
          SWP_NOSIZE = 0x0001;

        public const int
          SWP_NOZORDER = 0x0004;

        public const int
          TRANSPARENT = 1;

        public const int
          TVM_GETEDITCONTROL = (0x1100 + 15);

        public const int
          TVM_SETINSERTMARK = (0x1100 + 26);

        public const int WA_ACTIVE = 1,
                         WA_CLICKACTIVE = 2;

        public const int WA_INACTIVE = 0;
        public const int WHEEL_DELTA = 120;
        public const int WM_ACTIVATE = 0x0006;
        public const int WM_ACTIVATEAPP = 0x001C;

        public const int WM_AFXFIRST = 0x0360,
                         WM_AFXLAST = 0x037F;

        public const int WM_APP = unchecked(0x8000);
        public const int WM_ASKCBFORMATNAME = 0x030C;
        public const int WM_CANCELJOURNAL = 0x004B;
        public const int WM_CANCELMODE = 0x001F;
        public const int WM_CAPTURECHANGED = 0x0215;
        public const int WM_CHANGECBCHAIN = 0x030D;
        public const int WM_CHANGEUISTATE = 0x0127;
        public const int WM_CHAR = 0x0102;
        public const int WM_CHARTOITEM = 0x002F;
        public const int WM_CHILDACTIVATE = 0x0022;
        public const int WM_CHOOSEFONT_GETLOGFONT = (0x0400 + 1);
        public const int WM_CLEAR = 0x0303;
        public const int WM_CLOSE = 0x0010;
        public const int WM_COMMAND = 0x0111;
        public const int WM_COMMNOTIFY = 0x0044;
        public const int WM_COMPACTING = 0x0041;
        public const int WM_COMPAREITEM = 0x0039;
        public const int WM_CONTEXTMENU = 0x007B;
        public const int WM_COPY = 0x0301;
        public const int WM_COPYDATA = 0x004A;
        public const int WM_CREATE = 0x0001;
        public const int WM_CTLCOLOR = 0x0019;

        public const int WM_CTLCOLORBTN = 0x0135,
                         WM_CTLCOLORDLG = 0x0136;

        public const int WM_CTLCOLOREDIT = 0x0133,
                         WM_CTLCOLORLISTBOX = 0x0134;

        public const int WM_CTLCOLORMSGBOX = 0x0132;

        public const int WM_CTLCOLORSCROLLBAR = 0x0137,
                         WM_CTLCOLORSTATIC = 0x0138;

        public const int WM_CUT = 0x0300;
        public const int WM_DEADCHAR = 0x0103;

        public const int WM_DELETEITEM = 0x002D,
                         WM_DESTROY = 0x0002;

        public const int WM_DESTROYCLIPBOARD = 0x0307;
        public const int WM_DEVICECHANGE = 0x0219;
        public const int WM_DEVMODECHANGE = 0x001B;
        public const int WM_DISPLAYCHANGE = 0x007E;
        public const int WM_DRAWCLIPBOARD = 0x0308;
        public const int WM_DRAWITEM = 0x002B;
        public const int WM_DROPFILES = 0x0233;
        public const int WM_ENABLE = 0x000A;
        public const int WM_ENDSESSION = 0x0016;
        public const int WM_ENTERIDLE = 0x0121;
        public const int WM_ENTERMENULOOP = 0x0211;
        public const int WM_ENTERSIZEMOVE = 0x0231;
        public const int WM_ERASEBKGND = 0x0014;
        public const int WM_EXITMENULOOP = 0x0212;
        public const int WM_EXITSIZEMOVE = 0x0232;
        public const int WM_FONTCHANGE = 0x001D;
        public const int WM_GETDLGCODE = 0x0087;
        public const int WM_GETFONT = 0x0031;
        public const int WM_GETHOTKEY = 0x0033;
        public const int WM_GETICON = 0x007F;
        public const int WM_GETMINMAXINFO = 0x0024;
        public const int WM_GETOBJECT = 0x003D;

        public const int WM_GETTEXT = 0x000D,
                         WM_GETTEXTLENGTH = 0x000E;

        public const int WM_HANDHELDFIRST = 0x0358,
                         WM_HANDHELDLAST = 0x035F;

        public const int WM_HELP = 0x0053;
        public const int WM_HOTKEY = 0x0312;
        public const int WM_HSCROLL = 0x0114;
        public const int WM_HSCROLLCLIPBOARD = 0x030E;
        public const int WM_ICONERASEBKGND = 0x0027;
        public const int WM_IME_CHAR = 0x0286;
        public const int WM_IME_COMPOSITION = 0x010F;
        public const int WM_IME_COMPOSITIONFULL = 0x0284;
        public const int WM_IME_CONTROL = 0x0283;
        public const int WM_IME_ENDCOMPOSITION = 0x010E;
        public const int WM_IME_KEYDOWN = 0x0290;
        public const int WM_IME_KEYLAST = 0x010F;
        public const int WM_IME_KEYUP = 0x0291;
        public const int WM_IME_NOTIFY = 0x0282;
        public const int WM_IME_SELECT = 0x0285;
        public const int WM_IME_SETCONTEXT = 0x0281;
        public const int WM_IME_STARTCOMPOSITION = 0x010D;
        public const int WM_INITDIALOG = 0x0110;

        public const int WM_INITMENU = 0x0116,
                         WM_INITMENUPOPUP = 0x0117;

        public const int WM_INPUTLANGCHANGE = 0x0051;
        public const int WM_INPUTLANGCHANGEREQUEST = 0x0050;
        public const int WM_KEYDOWN = 0x0100;
        public const int WM_KEYFIRST = 0x0100;
        public const int WM_KEYLAST = 0x0108;
        public const int WM_KEYUP = 0x0101;
        public const int WM_KILLFOCUS = 0x0008;
        public const int WM_LBUTTONDBLCLK = 0x0203;

        public const int WM_LBUTTONDOWN = 0x0201,
                         WM_LBUTTONUP = 0x0202;

        public const int WM_MBUTTONDBLCLK = 0x0209;

        public const int WM_MBUTTONDOWN = 0x0207,
                         WM_MBUTTONUP = 0x0208;

        public const int WM_MDIACTIVATE = 0x0222;
        public const int WM_MDICASCADE = 0x0227;

        public const int WM_MDICREATE = 0x0220,
                         WM_MDIDESTROY = 0x0221;

        public const int WM_MDIGETACTIVE = 0x0229;
        public const int WM_MDIICONARRANGE = 0x0228;
        public const int WM_MDIMAXIMIZE = 0x0225;
        public const int WM_MDINEXT = 0x0224;
        public const int WM_MDIREFRESHMENU = 0x0234;
        public const int WM_MDIRESTORE = 0x0223;
        public const int WM_MDISETMENU = 0x0230;
        public const int WM_MDITILE = 0x0226;
        public const int WM_MEASUREITEM = 0x002C;
        public const int WM_MENUCHAR = 0x0120;
        public const int WM_MENUSELECT = 0x011F;
        public const int WM_MOUSEACTIVATE = 0x0021;
        public const int WM_MOUSEFIRST = 0x0200;
        public const int WM_MOUSEHOVER = 0x02A1;
        public const int WM_MOUSELAST = 0x020A;
        public const int WM_MOUSELEAVE = 0x02A3;
        public const int WM_MOUSEMOVE = 0x0200;
        public const int WM_MOUSEWHEEL = 0x020A;
        public const int WM_MOVE = 0x0003;
        public const int WM_MOVING = 0x0216;
        public const int WM_NCACTIVATE = 0x0086;
        public const int WM_NCCALCSIZE = 0x0083;

        public const int WM_NCCREATE = 0x0081,
                         WM_NCDESTROY = 0x0082;

        public const int WM_NCHITTEST = 0x0084;
        public const int WM_NCLBUTTONDBLCLK = 0x00A3;

        public const int WM_NCLBUTTONDOWN = 0x00A1,
                         WM_NCLBUTTONUP = 0x00A2;

        public const int WM_NCMBUTTONDBLCLK = 0x00A9;

        public const int WM_NCMBUTTONDOWN = 0x00A7,
                         WM_NCMBUTTONUP = 0x00A8;

        public const int WM_NCMOUSEMOVE = 0x00A0;
        public const int WM_NCPAINT = 0x0085;
        public const int WM_NCRBUTTONDBLCLK = 0x00A6;

        public const int WM_NCRBUTTONDOWN = 0x00A4,
                         WM_NCRBUTTONUP = 0x00A5;

        public const int WM_NCXBUTTONDBLCLK = 0x00AD;

        public const int WM_NCXBUTTONDOWN = 0x00AB,
                         WM_NCXBUTTONUP = 0x00AC;

        public const int WM_NEXTDLGCTL = 0x0028;
        public const int WM_NEXTMENU = 0x0213;
        public const int WM_NOTIFY = 0x004E;
        public const int WM_NOTIFYFORMAT = 0x0055;
        public const int WM_NULL = 0x0000;
        public const int WM_PAINT = 0x000F;
        public const int WM_PAINTCLIPBOARD = 0x0309;
        public const int WM_PAINTICON = 0x0026;
        public const int WM_PALETTECHANGED = 0x0311;
        public const int WM_PALETTEISCHANGING = 0x0310;
        public const int WM_PARENTNOTIFY = 0x0210;
        public const int WM_PASTE = 0x0302;

        public const int WM_PENWINFIRST = 0x0380,
                         WM_PENWINLAST = 0x038F;

        public const int WM_POWER = 0x0048;
        public const int WM_POWERBROADCAST = 0x0218;

        public const int WM_PRINT = 0x0317,
                         WM_PRINTCLIENT = 0x0318;

        public const int WM_QUERYDRAGICON = 0x0037;
        public const int WM_QUERYENDSESSION = 0x0011;
        public const int WM_QUERYNEWPALETTE = 0x030F;
        public const int WM_QUERYOPEN = 0x0013;
        public const int WM_QUERYUISTATE = 0x0129;
        public const int WM_QUEUESYNC = 0x0023;
        public const int WM_QUIT = 0x0012;
        public const int WM_RBUTTONDBLCLK = 0x0206;

        public const int WM_RBUTTONDOWN = 0x0204,
                         WM_RBUTTONUP = 0x0205;

        public const int WM_REFLECT =
          WM_USER + 0x1C00;

        public const int WM_RENDERALLFORMATS = 0x0306;
        public const int WM_RENDERFORMAT = 0x0305;
        public const int WM_SETCURSOR = 0x0020;
        public const int WM_SETFOCUS = 0x0007;
        public const int WM_SETFONT = 0x0030;
        public const int WM_SETHOTKEY = 0x0032;
        public const int WM_SETICON = 0x0080;

        public const int WM_SETREDRAW = 0x000B,
                         WM_SETTEXT = 0x000C;

        public const int WM_SETTINGCHANGE = 0x001A;
        public const int WM_SHOWWINDOW = 0x0018;
        public const int WM_SIZE = 0x0005;
        public const int WM_SIZECLIPBOARD = 0x030B;
        public const int WM_SIZING = 0x0214;
        public const int WM_SPOOLERSTATUS = 0x002A;
        public const int WM_STYLECHANGED = 0x007D;
        public const int WM_STYLECHANGING = 0x007C;
        public const int WM_SYSCHAR = 0x0106;
        public const int WM_SYSCOLORCHANGE = 0x0015;
        public const int WM_SYSCOMMAND = 0x0112;
        public const int WM_SYSDEADCHAR = 0x0107;

        public const int WM_SYSKEYDOWN = 0x0104,
                         WM_SYSKEYUP = 0x0105;

        public const int WM_TCARD = 0x0052;
        public const int WM_TIMECHANGE = 0x001E;
        public const int WM_TIMER = 0x0113;
        public const int WM_UNDO = 0x0304;
        public const int WM_UPDATEUISTATE = 0x0128;
        public const int WM_USER = 0x0400;
        public const int WM_USERCHANGED = 0x0054;
        public const int WM_VKEYTOITEM = 0x002E;
        public const int WM_VSCROLL = 0x0115;
        public const int WM_VSCROLLCLIPBOARD = 0x030A;
        public const int WM_WINDOWPOSCHANGED = 0x0047;
        public const int WM_WINDOWPOSCHANGING = 0x0046;
        public const int WM_WININICHANGE = 0x001A;
        public const int WM_XBUTTONDBLCLK = 0x020D;

        public const int WM_XBUTTONDOWN = 0x020B,
                         WM_XBUTTONUP = 0x020C;

        public const int WPF_SETMINPOSITION = 0x0001;

        public const int WS_BORDER = 0x800000;
        public const int WS_CAPTION = 0xc00000;
        public const int WS_CHILD = 0x40000000;
        public const int WS_CLIPCHILDREN = 0x2000000;
        public const int WS_CLIPSIBLINGS = 0x4000000;
        public const int WS_DISABLED = 0x8000000;
        public const int WS_DLGFRAME = 0x400000;
        public const int WS_EX_APPWINDOW = 0x40000;
        public const int WS_EX_CLIENTEDGE = 0x200;
        public const int WS_EX_CONTEXTHELP = 0x400;
        public const int WS_EX_CONTROLPARENT = 0x10000;
        public const int WS_EX_DLGMODALFRAME = 1;
        public const int WS_EX_LAYERED = 0x80000;
        public const int WS_EX_LEFT = 0;
        public const int WS_EX_LEFTSCROLLBAR = 0x4000;
        public const int WS_EX_MDICHILD = 0x40;
        public const int WS_EX_NOPARENTNOTIFY = 4;
        public const int WS_EX_RIGHT = 0x1000;
        public const int WS_EX_RTLREADING = 0x2000;
        public const int WS_EX_STATICEDGE = 0x20000;
        public const int WS_EX_TOOLWINDOW = 0x80;
        public const int WS_EX_TOPMOST = 8;
        public const int WS_HSCROLL = 0x100000;
        public const int WS_MAXIMIZE = 0x1000000;
        public const int WS_MAXIMIZEBOX = 0x10000;
        public const int WS_MINIMIZE = 0x20000000;
        public const int WS_MINIMIZEBOX = 0x20000;
        public const int WS_OVERLAPPED = 0x00000000;
        public const int WS_POPUP = -2147483648;
        public const int WS_SYSMENU = 0x80000;
        public const int WS_TABSTOP = 0x10000;
        public const int WS_THICKFRAME = 0x40000;
        public const int WS_VISIBLE = 0x10000000;
        public const int WS_VSCROLL = 0x200000;
        public const int WSF_VISIBLE = 0x0001;
        public static readonly Guid IID_IObjectWithSite = typeof(IObjectWithSite).GUID;
        public static readonly Guid IID_IServiceProvider = typeof(IServiceProvider).GUID;
        public static readonly Guid IID_IUnknown = new Guid("{00000000-0000-0000-C000-000000000046}");

        /// <devdoc>
        /// This method takes a file URL and converts it to an absolute path.  The trick here is that
        /// if there is a '#' in the path, everything after this is treated as a fragment.  So
        /// we need to append the fragment to the end of the path.
        /// </devdoc>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static string GetAbsolutePath(string fileName)
        {
            Debug.Assert(!string.IsNullOrEmpty(fileName),
              "Cannot get absolute path, fileName is not valid");

            var uri = new Uri(fileName);
            return uri.LocalPath + uri.Fragment;
        }

        /// <devdoc>
        /// This method takes a file URL and converts it to a local path.  The trick here is that
        /// if there is a '#' in the path, everything after this is treated as a fragment.  So
        /// we need to append the fragment to the end of the path.
        /// </devdoc>
        public static string GetLocalPath(string fileName)
        {
            Debug.Assert(!string.IsNullOrEmpty(fileName), "Cannot get local path, fileName is not valid");

            var uri = new Uri(fileName);
            return uri.LocalPath + uri.Fragment;
        }

        /// <summary>
        /// Gets the icon.
        /// </summary>
        /// <param name="pszPath">The PSZ path.</param>
        /// <returns></returns>
        public static Icon GetIcon(string pszPath)
        {
            var shinfo = new SHFILEINFO();

            SHGetFileInfo(pszPath, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), SHGFI_ICON | SHGFI_SMALLICON);

            if ((int)shinfo.hIcon == 0)
                return null;

            try
            {
                Icon myIcon = Icon.FromHandle(shinfo.hIcon);
                return myIcon;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        /// <devdoc>
        /// Please use this "approved" method to compare file names.
        /// </devdoc>
        public static bool IsSamePath(string file1, string file2)
        {
            if (string.IsNullOrEmpty(file1))
            {
                return (string.IsNullOrEmpty(file2));
            }

            Uri uri1;
            Uri uri2;

            try
            {
                if (!Uri.TryCreate(file1, UriKind.Absolute, out uri1) || !Uri.TryCreate(file2, UriKind.Absolute, out uri2))
                {
                    return false;
                }

                if (uri1 != null && uri1.IsFile && uri2 != null && uri2.IsFile)
                {
                    return 0 == String.Compare(uri1.LocalPath, uri2.LocalPath, StringComparison.OrdinalIgnoreCase);
                }

                return file1 == file2;
            }
            catch (UriFormatException e)
            {
                Trace.WriteLine("Exception " + e.Message);
            }

            return false;
        }


        // APIS

        /// <summary>
        /// Changes the parent window of the specified child window.
        /// </summary>
        /// <param name="hWnd">Handle to the child window.</param>
        /// <param name="hWndParent">Handle to the new parent window. If this parameter is NULL, the desktop window becomes the new parent window.</param>
        /// <returns>A handle to the previous parent window indicates success. NULL indicates failure.</returns>
        [DllImport("User32", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SetParent(IntPtr hWnd, IntPtr hWndParent);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern bool DestroyIcon(IntPtr handle);

        [DllImport("user32.dll", EntryPoint = "IsDialogMessageA", SetLastError = true, CharSet = CharSet.Ansi,
          ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool IsDialogMessageA(IntPtr hDlg, ref MSG msg);

        /// <summary>
        /// Indicates whether the file type is binary or not
        /// </summary>
        /// <param name="lpApplicationName">Full path to the file to check</param>
        /// <param name="lpBinaryType">If file isbianry the bitness of the app is indicated by lpBinaryType value.</param>
        /// <returns>True if the file is binary false otherwise</returns>
        [DllImport("kernel32.dll")]
        public static extern bool GetBinaryType([MarshalAs(UnmanagedType.LPWStr)] string lpApplicationName,
                                                out uint lpBinaryType);


        public static bool Succeeded(int hr)
        {
            return (hr >= 0);
        }

        public static bool Failed(int hr)
        {
            return (hr < 0);
        }

        public static int ThrowOnFailure(int hr)
        {
            return ThrowOnFailure(hr, null);
        }

        public static int ThrowOnFailure(int hr, params int[] expectedHRFailure)
        {
            if (Failed(hr) && ((expectedHRFailure == null) || (Array.IndexOf(expectedHRFailure, hr) < 0)))
            {
                Marshal.ThrowExceptionForHR(hr);
            }
            return hr;
        }

        /// <summary>
        /// Get file info.
        /// </summary>
        /// <param name="pszPath">The PSZ path.</param>
        /// <param name="dwFileAttributes">The dw file attributes.</param>
        /// <param name="psfi">The psfi.</param>
        /// <param name="cbSizeFileInfo">The cb size file info.</param>
        /// <param name="uFlags">The u flags.</param>
        /// <returns></returns>
        [DllImport("shell32.dll")]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        /// <summary>
        /// Get image count.
        /// </summary>
        /// <param name="HIMAGELIST">The HIMAGELIST.</param>
        /// <returns></returns>
        [DllImport("COMCTL32")]
        public static extern int ImageList_GetImageCount(int HIMAGELIST);

        /// <summary>
        /// Get icon.
        /// </summary>
        /// <param name="HIMAGELIST">The HIMAGELIST.</param>
        /// <param name="ImgIndex">Index of the img.</param>
        /// <param name="hbmMask">The HBM mask.</param>
        /// <returns></returns>
        [DllImport("COMCTL32")]
        public static extern int ImageList_GetIcon(int HIMAGELIST, int ImgIndex, int hbmMask);

        #region Nested type: ConnectionPointCookie

        /// <devdoc>
        /// Class that encapsulates a connection point cookie for COM event handling.
        /// </devdoc>
        public sealed class ConnectionPointCookie : IDisposable
        {
#if DEBUG
            private readonly string callStack = "(none)";
            private readonly Type eventInterface;
            private IConnectionPoint connectionPoint;
            private uint cookie;
            private IConnectionPointContainer cpc;
#endif

            /// <include file='doc\NativeMethods.uex' path='docs/doc[@for="ConnectionPointCookie.ConnectionPointCookie"]/*' />
            /// <devdoc>
            /// Creates a connection point to of the given interface type.
            /// which will call on a managed code sink that implements that interface.
            /// </devdoc>
            public ConnectionPointCookie(object source, object sink, Type eventInterface)
                : this(source, sink, eventInterface, true)
            {
            }

            /// <devdoc>
            /// Creates a connection point to of the given interface type.
            /// which will call on a managed code sink that implements that interface.
            /// </devdoc>
            public ConnectionPointCookie(object source, object sink, Type eventInterface, bool throwException)
            {
                Exception ex = null;
                if (source is IConnectionPointContainer)
                {
                    cpc = (IConnectionPointContainer)source;

                    try
                    {
                        Guid tmp = eventInterface.GUID;
                        cpc.FindConnectionPoint(ref tmp, out connectionPoint);
                    }
                    catch
                    {
                        connectionPoint = null;
                    }

                    if (connectionPoint == null)
                    {
                        ex = new ArgumentException( /* SR.GetString(SR.ConnectionPoint_SourceIF, eventInterface.Name)*/);
                    }
                    else if (sink == null || !eventInterface.IsInstanceOfType(sink))
                    {
                        ex = new InvalidCastException( /* SR.GetString(SR.ConnectionPoint_SinkIF)*/);
                    }
                    else
                    {
                        try
                        {
                            connectionPoint.Advise(sink, out cookie);
                        }
                        catch
                        {
                            cookie = 0;
                            connectionPoint = null;
                            ex = new Exception( /*SR.GetString(SR.ConnectionPoint_AdviseFailed, eventInterface.Name)*/);
                        }
                    }
                }
                else
                {
                    ex = new InvalidCastException( /*SR.ConnectionPoint_SourceNotICP)*/);
                }


                if (throwException && (connectionPoint == null || cookie == 0))
                {
                    if (ex == null)
                    {
                        throw new ArgumentException( /*SR.GetString(SR.ConnectionPoint_CouldNotCreate, eventInterface.Name)*/);
                    }
                    throw ex;
                }

#if DEBUG
                callStack = Environment.StackTrace;
                this.eventInterface = eventInterface;
#endif
            }

            #region IDisposable Members

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            #endregion

            /// <include file='doc\NativeMethods.uex' path='docs/doc[@for="NativeMethods.Finalize1"]/*' />
            ~ConnectionPointCookie()
            {
#if DEBUG
                Debug.Assert(connectionPoint == null || cookie == 0,
                             "We should never finalize an active connection point. (Interface = " + eventInterface.FullName +
                             "), allocating code (see stack) is responsible for unhooking the ConnectionPoint by calling Disconnect.  Hookup Stack =\r\n" +
                             callStack);
#endif

                // We must pass false here because chances are that if someone
                // forgot to Dispose this object, the IConnectionPoint is likely to be 
                // a disconnected RCW at this point (for example, we are far along in a 
                // VS shutdown scenario).  The result will be a memory leak, which is the 
                // expected result for an undisposed IDisposable object. [clovett] bug 369592.
                Dispose(false);
            }

            private void Dispose(bool disposing)
            {
                if (disposing)
                {
                    try
                    {
                        if (connectionPoint != null && cookie != 0)
                        {
                            connectionPoint.Unadvise(cookie);
                        }
                    }
                    finally
                    {
                        cookie = 0;
                        connectionPoint = null;
                        cpc = null;
                    }
                }
            }
        }

        #endregion

        #region Nested type: DataStreamFromComStream

        /// <devdoc>
        /// This class implements a managed Stream object on top
        /// of a COM IStream
        /// </devdoc>
        internal sealed class DataStreamFromComStream : Stream
        {
#if DEBUG
            private readonly string creatingStack;
            private IStream comStream;
#endif

            public DataStreamFromComStream(IStream comStream)
            {
                this.comStream = comStream;

#if DEBUG
                creatingStack = Environment.StackTrace;
#endif
            }

            public override long Position
            {
                get { return Seek(0, SeekOrigin.Current); }

                set { Seek(value, SeekOrigin.Begin); }
            }

            public override bool CanWrite
            {
                get { return true; }
            }

            public override bool CanSeek
            {
                get { return true; }
            }

            public override bool CanRead
            {
                get { return true; }
            }

            public override long Length
            {
                get
                {
                    long curPos = Position;
                    long endPos = Seek(0, SeekOrigin.End);
                    Position = curPos;
                    return endPos - curPos;
                }
            }

            protected override void Dispose(bool disposing)
            {
                try
                {
                    if (disposing)
                    {
                        if (comStream != null)
                        {
                            Flush();
                        }
                    }
                    // Cannot close COM stream from finalizer thread.
                    comStream = null;
                }
                finally
                {
                    base.Dispose(disposing);
                }
            }

            public override void Flush()
            {
                if (comStream != null)
                {
                    try
                    {
                        comStream.Commit(StreamConsts.STGC_DEFAULT);
                    }
                    catch
                    {
                    }
                }
            }

            public override int Read(byte[] buffer, int index, int count)
            {
                uint bytesRead;
                byte[] b = buffer;

                if (index != 0)
                {
                    b = new byte[buffer.Length - index];
                }

                comStream.Read(b, (uint)count, out bytesRead);

                if (index != 0)
                {
                    b.CopyTo(buffer, index);
                }

                return (int)bytesRead;
            }

            public override void SetLength(long value)
            {
                var ul = new ULARGE_INTEGER { QuadPart = ((ulong)value) };
                comStream.SetSize(ul);
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                var l = new LARGE_INTEGER();
                var ul = new ULARGE_INTEGER[1];
                ul[0] = new ULARGE_INTEGER();
                l.QuadPart = offset;
                comStream.Seek(l, (uint)origin, ul);
                return (long)ul[0].QuadPart;
            }

            public override void Write(byte[] buffer, int index, int count)
            {
                if (count > 0)
                {
                    byte[] b = buffer;

                    if (index != 0)
                    {
                        b = new byte[buffer.Length - index];
                        buffer.CopyTo(b, 0);
                    }

                    uint bytesWritten;
                    comStream.Write(b, (uint)count, out bytesWritten);
                    if (bytesWritten != count)
                        // Didn't write enough bytes to IStream!
                        throw new IOException();

                    if (index != 0)
                    {
                        b.CopyTo(buffer, index);
                    }
                }
            }

            ~DataStreamFromComStream()
            {
#if DEBUG
                if (comStream != null)
                {
                    Debug.Fail("DataStreamFromComStream not closed.  Creating stack: " + creatingStack);
                }
#endif
                // CANNOT CLOSE NATIVE STREAMS IN FINALIZER THREAD
                // Close();
            }
        }

        #endregion

        #region Nested type: ICodeClassBase

        ///--------------------------------------------------------------------------
        /// ICodeClassBase:
        ///--------------------------------------------------------------------------
        [Guid("23BBD58A-7C59-449b-A93C-43E59EFC080C")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [ComImport]
        public interface ICodeClassBase
        {
            [PreserveSig]
            int GetBaseName(out string pBaseName);
        }

        #endregion

        #region Nested type: IEventHandler

        [ComImport, Guid("9BDA66AE-CA28-4e22-AA27-8A7218A0E3FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IEventHandler
        {
            // converts the underlying codefunction into an event handler for the given event
            // if the given event is NULL, then the function will handle no events
            [PreserveSig]
            int AddHandler(string bstrEventName);

            [PreserveSig]
            int RemoveHandler(string bstrEventName);

            IVsEnumBSTR GetHandledEvents();

            bool HandlesEvent(string bstrEventName);
        }

        #endregion

        #region Nested type: IMethodXML

        [
          ComImport, ComVisible(true), Guid("3E596484-D2E4-461a-A876-254C4F097EBB"),
          InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
        ]
        public interface IMethodXML
        {
            // Generate XML describing the contents of this function's body.
            void GetXML(ref string pbstrXML);

            // Parse the incoming XML with respect to the CodeModel XML schema and
            // use the result to regenerate the body of the function.
            /// <include file='doc\NativeMethods.uex' path='docs/doc[@for="IMethodXML.SetXML"]/*' />
            [PreserveSig]
            int SetXML(string pszXML);

            // This is really a textpoint
            [PreserveSig]
            int GetBodyPoint([MarshalAs(UnmanagedType.Interface)] out object bodyPoint);
        }

        #endregion

        #region Nested type: IParameterKind

        [ComImport, Guid("A55CCBCC-7031-432d-B30A-A68DE7BDAD75"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IParameterKind
        {
            void SetParameterPassingMode(PARAMETER_PASSING_MODE ParamPassingMode);
            void SetParameterArrayDimensions(int uDimensions);
            int GetParameterArrayCount();
            int GetParameterArrayDimensions(int uIndex);
            int GetParameterPassingMode();
        }

        #endregion

        #region Nested type: IVBFileCodeModelEvents

        [ComImport, Guid("EA1A87AD-7BC5-4349-B3BE-CADC301F17A3"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IVBFileCodeModelEvents
        {
            [PreserveSig]
            int StartEdit();

            [PreserveSig]
            int EndEdit();
        }

        #endregion

        #region Nested type: LOGFONT

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class LOGFONT
        {
            public int lfHeight;
            public int lfWidth;
            public int lfEscapement;
            public int lfOrientation;
            public int lfWeight;
            public byte lfItalic;
            public byte lfUnderline;
            public byte lfStrikeOut;
            public byte lfCharSet;
            public byte lfOutPrecision;
            public byte lfClipPrecision;
            public byte lfQuality;
            public byte lfPitchAndFamily;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string lfFaceName;
        }

        #endregion

        #region Nested type: NMHDR

        [StructLayout(LayoutKind.Sequential)]
        public struct NMHDR
        {
            public IntPtr hwndFrom;
            public int idFrom;
            public int code;
        }

        #endregion

        #region Nested type: OLECMDTEXT

        /// <devdoc>
        /// Helper class for setting the text parameters to OLECMDTEXT structures.
        /// </devdoc>
        public static class OLECMDTEXT
        {
            #region OLECMDTEXTF enum

            /// <summary>
            /// Flags for the OLE command text
            /// </summary>
            public enum OLECMDTEXTF
            {
                /// <summary>No flag</summary>
                OLECMDTEXTF_NONE = 0,
                /// <summary>The name of the command is required.</summary>
                OLECMDTEXTF_NAME = 1,
                /// <summary>A description of the status is required.</summary>
                OLECMDTEXTF_STATUS = 2
            }

            #endregion

            /// <summary>
            /// Gets the flags of the OLECMDTEXT structure
            /// </summary>
            /// <param name="pCmdTextInt">The structure to read.</param>
            /// <returns>The value of the flags.</returns>
            public static OLECMDTEXTF GetFlags(IntPtr pCmdTextInt)
            {
                var pCmdText =
                  (Microsoft.VisualStudio.OLE.Interop.OLECMDTEXT)
                  Marshal.PtrToStructure(pCmdTextInt, typeof(Microsoft.VisualStudio.OLE.Interop.OLECMDTEXT));

                if ((pCmdText.cmdtextf & (int)OLECMDTEXTF.OLECMDTEXTF_NAME) != 0)
                    return OLECMDTEXTF.OLECMDTEXTF_NAME;

                if ((pCmdText.cmdtextf & (int)OLECMDTEXTF.OLECMDTEXTF_STATUS) != 0)
                    return OLECMDTEXTF.OLECMDTEXTF_STATUS;

                return OLECMDTEXTF.OLECMDTEXTF_NONE;
            }

            /// <devdoc>
            /// Accessing the text of this structure is very cumbersome.  Instead, you may
            /// use this method to access an integer pointer of the structure.
            /// Passing integer versions of this structure is needed because there is no
            /// way to tell the common language runtime that there is extra data at the end of the structure.
            /// </devdoc>
            public static string GetText(IntPtr pCmdTextInt)
            {
                var pCmdText =
                  (Microsoft.VisualStudio.OLE.Interop.OLECMDTEXT)
                  Marshal.PtrToStructure(pCmdTextInt, typeof(Microsoft.VisualStudio.OLE.Interop.OLECMDTEXT));

                // Get the offset to the rgsz param.
                //
                IntPtr offset = Marshal.OffsetOf(typeof(Microsoft.VisualStudio.OLE.Interop.OLECMDTEXT), "rgwz");

                // Punt early if there is no text in the structure.
                //
                if (pCmdText.cwActual == 0)
                {
                    return String.Empty;
                }

                var text = new char[pCmdText.cwActual - 1];

                Marshal.Copy((IntPtr)((long)pCmdTextInt + (long)offset), text, 0, text.Length);

                var s = new StringBuilder(text.Length);
                s.Append(text);
                return s.ToString();
            }

            /// <include file='doc\NativeMethods.uex' path='docs/doc[@for="OLECMDTEXTF.SetText"]/*' />
            /// <devdoc>
            /// Accessing the text of this structure is very cumbersome.  Instead, you may
            /// use this method to access an integer pointer of the structure.
            /// Passing integer versions of this structure is needed because there is no
            /// way to tell the common language runtime that there is extra data at the end of the structure.
            /// </devdoc>
            /// <summary>
            /// Sets the text inside the structure starting from an integer pointer.
            /// </summary>
            /// <param name="pCmdTextInt">The integer pointer to the position where to set the text.</param>
            /// <param name="text">The text to set.</param>
            public static void SetText(IntPtr pCmdTextInt, string text)
            {
                var pCmdText =
                  (Microsoft.VisualStudio.OLE.Interop.OLECMDTEXT)
                  Marshal.PtrToStructure(pCmdTextInt, typeof(Microsoft.VisualStudio.OLE.Interop.OLECMDTEXT));
                char[] menuText = text.ToCharArray();

                // Get the offset to the rgsz param.  This is where we will stuff our text
                //
                IntPtr offset = Marshal.OffsetOf(typeof(Microsoft.VisualStudio.OLE.Interop.OLECMDTEXT), "rgwz");
                IntPtr offsetToCwActual = Marshal.OffsetOf(typeof(Microsoft.VisualStudio.OLE.Interop.OLECMDTEXT), "cwActual");

                // The max chars we copy is our string, or one less than the buffer size,
                // since we need a null at the end.
                //
                int maxChars = Math.Min((int)pCmdText.cwBuf - 1, menuText.Length);

                Marshal.Copy(menuText, 0, (IntPtr)((long)pCmdTextInt + (long)offset), maxChars);

                // append a null character
                Marshal.WriteInt16((IntPtr)((long)pCmdTextInt + (long)offset + maxChars * 2), 0);

                // write out the length
                // +1 for the null char
                Marshal.WriteInt32((IntPtr)((long)pCmdTextInt + (long)offsetToCwActual), maxChars + 1);
            }
        }

        #endregion

        #region Nested type: StreamConsts

        /// <devdoc>
        /// Constants for stream usage.
        /// </devdoc>
        public sealed class StreamConsts
        {
            public const int LOCK_EXCLUSIVE = 0x2;
            public const int LOCK_ONLYONCE = 0x4;
            public const int LOCK_WRITE = 0x1;
            public const int STATFLAG_DEFAULT = 0x0;
            public const int STATFLAG_NONAME = 0x1;
            public const int STATFLAG_NOOPEN = 0x2;
            public const int STGC_DANGEROUSLYCOMMITMERELYTODISKCACHE = 0x4;
            public const int STGC_DEFAULT = 0x0;
            public const int STGC_ONLYIFCURRENT = 0x2;
            public const int STGC_OVERWRITE = 0x1;
            public const int STREAM_SEEK_CUR = 0x1;
            public const int STREAM_SEEK_END = 0x2;
            public const int STREAM_SEEK_SET = 0x0;
        }

        #endregion
    }
    /// <summary>
    /// Static class for unsafe native P/Invoke methods and types
    /// </summary>
    // ================================================================================================
    [SuppressUnmanagedCodeSecurity]
    internal class UnsafeNativeMethods
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern int CloseClipboard();
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern bool CloseHandle(HandleRef handle);
        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory", ExactSpelling = true)]
        internal static extern void CopyMemory(IntPtr pdst, HandleRef psrc, int cb);
        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory", ExactSpelling = true)]
        internal static extern void CopyMemory(IntPtr pdst, string psrc, int cb);
        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory", ExactSpelling = true)]
        internal static extern void CopyMemory(byte[] pdst, HandleRef psrc, int cb);
        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory", ExactSpelling = true)]
        internal static extern void CopyMemory(IntPtr pdst, byte[] psrc, int cb);
        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory", CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern void CopyMemoryW(IntPtr pdst, char[] psrc, int cb);
        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory", CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern void CopyMemoryW(IntPtr pdst, string psrc, int cb);
        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory", CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern void CopyMemoryW(char[] pdst, HandleRef psrc, int cb);
        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory", CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern void CopyMemoryW(StringBuilder pdst, HandleRef psrc, int cb);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern void DebugBreak();
        [DllImport("shell32.dll", EntryPoint = "DragQueryFileW", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        public static extern uint DragQueryFile(IntPtr hDrop, uint iFile, char[] lpszFile, uint cch);
        [DllImport("user32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern int EmptyClipboard();
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern int GetFileAttributes(string name);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern void GetTempFileName(string tempDirName, string prefixName, int unique, StringBuilder sb);
        public static IntPtr GetWindowLong(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 4)
            {
                return GetWindowLong32(hWnd, nIndex);
            }
            return GetWindowLongPtr64(hWnd, nIndex);
        }

        [DllImport("user32.dll", EntryPoint = "GetWindowLong", CharSet = CharSet.Auto)]
        public static extern IntPtr GetWindowLong32(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", CharSet = CharSet.Auto)]
        public static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        internal static extern IntPtr GlobalAlloc(int uFlags, int dwBytes);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        internal static extern IntPtr GlobalFree(HandleRef handle);
        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        internal static extern IntPtr GlobalLock(IntPtr h);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        internal static extern IntPtr GlobalLock(HandleRef handle);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        internal static extern IntPtr GlobalReAlloc(HandleRef handle, int bytes, int flags);
        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        internal static extern int GlobalSize(IntPtr h);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        internal static extern int GlobalSize(HandleRef handle);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        internal static extern bool GlobalUnlock(HandleRef handle);
        [DllImport("kernel32.dll", EntryPoint = "GlobalUnlock", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        internal static extern bool GlobalUnLock(IntPtr h);
        [DllImport("comctl32.dll", CharSet = CharSet.Auto)]
        public static extern bool ImageList_Draw(HandleRef himl, int i, HandleRef hdcDst, int x, int y, int fStyle);
        [DllImport("comctl32.dll", CharSet = CharSet.Auto)]
        public static extern int ImageList_GetImageCount(HandleRef himl);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool LoadString(HandleRef hInstance, int uID, StringBuilder lpBuffer, int nBufferMax);
        [DllImport("ole32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern int OleFlushClipboard();
        [DllImport("ole32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern int OleGetClipboard(out IDataObject dataObject);
        [DllImport("ole32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern int OleSetClipboard(IDataObject dataObject);
        [DllImport("user32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern int OpenClipboard(IntPtr newOwner);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);
        [DllImport("user32.dll", EntryPoint = "RegisterClipboardFormatW", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        internal static extern ushort RegisterClipboardFormat(string format);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hwnd, int msg, bool wparam, int lparam);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, string lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        internal static extern IntPtr SetParent(IntPtr hWnd, IntPtr hWndParent);
        public static IntPtr SetWindowLong(IntPtr hWnd, short nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 4)
            {
                return SetWindowLongPtr32(hWnd, nIndex, dwNewLong);
            }
            return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
        }

        public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 4)
            {
                return SetWindowLongPtr32(hWnd, nIndex, dwNewLong);
            }
            return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Auto)]
        public static extern IntPtr SetWindowLongPtr32(IntPtr hWnd, short nIndex, IntPtr dwNewLong);
        [DllImport("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Auto)]
        public static extern IntPtr SetWindowLongPtr32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", CharSet = CharSet.Auto)]
        public static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int flags);
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern int WideCharToMultiByte(int codePage, int flags, [MarshalAs(UnmanagedType.LPWStr)] string wideStr, int chars, [In, Out] byte[] pOutBytes, int bufferBytes, IntPtr defaultChar, IntPtr pDefaultUsed);
    }

}
