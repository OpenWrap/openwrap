using System.Runtime.InteropServices;

namespace OpenWrap.VisualStudio.SolutionAddIn
{
    [Guid(ComConstants.ADD_IN_GUID_2008)]
    [ProgId(ComConstants.ADD_IN_PROGID_2008)]
    [ComVisible(true)]
    public class OpenWrapVisualStudioAddIn2008 : OpenWrapVisualStudioAddIn
    {
        public OpenWrapVisualStudioAddIn2008() : base("9.0", ComConstants.ADD_IN_PROGID_2008) { }
    }
}