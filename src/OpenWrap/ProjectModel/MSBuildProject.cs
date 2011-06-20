using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using SysFile = System.IO.File;

namespace OpenWrap.ProjectModel.Drivers.File
{
    public static class MSBuildProject
    {
        const string MSBUILD_NS = "http://schemas.microsoft.com/developer/msbuild/2003";
        public static bool OpenWrapEnabled(string filePath)
        {
            if (!SysFile.Exists(filePath))
                return false;
            var xmlDoc = new XmlDocument();
            var namespaceManager = new XmlNamespaceManager(xmlDoc.NameTable);
            namespaceManager.AddNamespace("msbuild", MSBUILD_NS);

            using (var projectFileStream = SysFile.OpenRead(filePath))
                xmlDoc.Load(projectFileStream);
            var isOpenWrap = (from node in xmlDoc.SelectNodes(@"//msbuild:Import", namespaceManager).OfType<XmlElement>()
                              let attr = node.GetAttribute("Project")
                              where attr != null && Regex.IsMatch(attr, @"OpenWrap\..*\.targets")
                              select node).Any();

            var isDisabled =
                    (
                            from node in xmlDoc.SelectNodes(@"//msbuild:OpenWrap-EnableVisualStudioIntegration", namespaceManager).OfType<XmlElement>()
                            let value = node.Value
                            where value != null && value.EqualsNoCase("false")
                            select node
                    ).Any();

            return isOpenWrap && !isDisabled;
        }
    }
}