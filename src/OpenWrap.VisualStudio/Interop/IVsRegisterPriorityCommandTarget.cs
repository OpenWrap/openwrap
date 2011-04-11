using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.OLE.Interop;

namespace OpenWrap.VisualStudio.Interop
{
    [ComImport, Guid("ED77D5EC-B0DE-4721-BDC6-38DCBE589B4C"), InterfaceType((short)1)]
    public interface IVsRegisterPriorityCommandTarget
    {
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int RegisterPriorityCommandTarget([In, ComAliasName("Microsoft.VisualStudio.OLE.Interop.DWORD")] uint dwReserved, [In, MarshalAs(UnmanagedType.Interface)] IOleCommandTarget pCmdTrgt, [ComAliasName("Microsoft.VisualStudio.Shell.Interop.VSCOOKIE")] out uint pdwCookie);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int UnregisterPriorityCommandTarget([In, ComAliasName("Microsoft.VisualStudio.Shell.Interop.VSCOOKIE")] uint dwCookie);
    }
}