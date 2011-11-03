using System;
using System.Xml;
using OpenWrap.PackageModel;
using OpenWrap.Repositories.NuGet;

namespace Tests.Repositories.contexts
{
    public abstract class nuspec : OpenWrap.Testing.context
    {
        const string template =
            @"<?xml version=""1.0"" encoding=""utf-8""?>
<package>
  <metadata>
    <id>{0}</id>
    <version>{1}</version>
    <authors>{2}</authors>
    <description>{3}</description>
    <title>{4}</title>
    <owners>{5}</owners>
    <projectUrl>{6}</projectUrl>
    <licenseUrl>{7}</licenseUrl>
    <requireLicenseAcceptance>{8}</requireLicenseAcceptance>
    <tags>{9}</tags>
    <dependencies>
      <dependency id=""first"" version=""3.0.0"" />
      <dependency id=""second""/>
      <dependency id=""third"" version=""[1.0.0)"" />
    </dependencies> 
  </metadata>
</package>";

        protected PackageDescriptor given_spec(string id, string version, string authors, string description, string title = null, string owners = null, string projectUrl = null, string licenseUrl = null, string requireLicenseAcceptance = null, string tags = null)
        {
            var spec = String.Format(template, id, version, authors, description, title, owners, projectUrl, licenseUrl, requireLicenseAcceptance, tags);
            var doc = new XmlDocument();
            doc.LoadXml(spec);
            return Descriptor =  NuConverter.ConvertSpecificationToDescriptor(doc);
        }

        protected PackageDescriptor Descriptor { get; set; }
    }
}