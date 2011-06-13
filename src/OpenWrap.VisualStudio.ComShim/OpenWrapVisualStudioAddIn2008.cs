using System.Runtime.InteropServices;

namespace OpenWrap.VisualStudio.ComShim
{
    [Guid(Constants.ADD_IN_GUID_2008)]
    [ProgId(Constants.ADD_IN_PROGID_2008)]
    [ComVisible(true)]
    public class OpenWrapVisualStudioAddIn2008 : OpenWrapVisualStudioAddIn
    {
        public OpenWrapVisualStudioAddIn2008() : base("9.0", Constants.ADD_IN_GUID_2008) { }
    }
        ;
}