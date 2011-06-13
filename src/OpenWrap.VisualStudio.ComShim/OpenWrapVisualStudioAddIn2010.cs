using System.Runtime.InteropServices;

namespace OpenWrap.VisualStudio.ComShim
{
    [Guid(Constants.ADD_IN_GUID_2010)]
    [ProgId(Constants.ADD_IN_PROGID_2010)]
    [ComVisible(true)]
    public class OpenWrapVisualStudioAddIn2010 : OpenWrapVisualStudioAddIn
    {
        public OpenWrapVisualStudioAddIn2010() : base("10.0", Constants.ADD_IN_GUID_2010) { }
    }
}