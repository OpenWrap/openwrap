using System;
using System.Xml;
using System.Xml.Linq;
using OpenWrap;
using OpenWrap.Repositories.NuFeed;

namespace Tests.Repositories.factories.nuget
{
    public static class AtomContent
    {
        const string SVC_DOC =
                @"
<service xmlns=""http://www.w3.org/2007/app"" 
         xmlns:atom=""http://www.w3.org/2005/Atom""
         xml:base=""{0}"">
  <workspace>
    <atom:title>Default</atom:title>
    <collection href=""{1}"">
      <atom:title>Packages</atom:title>
    </collection>
  </workspace>
</service>
";

        const string FEED_DOC =
                @"
<feed xml:base=""{2}"" 
        xmlns=""http://www.w3.org/2005/Atom"">
  <title type=""text"">Packages</title>
  <id>{0}</id>
  <updated>{1}</updated>
{3}
</feed>
        ";

        const string NUGET_ENTRY_DOC =
                @"
<entry xmlns=""http://www.w3.org/2005/Atom"">
    <id>http://openwrap.org/somewhere/over/the/rainbow/{0}</id>
    <title type=""text"">{1}</title>
    <summary type=""text"">{2}</summary>
    <updated>{3}</updated>
    <author>
      <name>{4}</name>
    </author>
    
    <content type=""application/zip"" src=""{5}"" />
    <m:properties xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"" xmlns:d=""http://schemas.microsoft.com/ado/2007/08/dataservices"">
      <d:Id>{6}</d:Id>
      <d:Version>{7}</d:Version>
      <d:Title>{8}</d:Title>
      <d:Authors>{4}</d:Authors>
      
      <d:Summary>{2}</d:Summary>
      
      <d:Copyright m:null=""true""></d:Copyright>
      <d:RequireLicenseAcceptance m:type=""Edm.Boolean"">false</d:RequireLicenseAcceptance>
      <d:Created m:type=""Edm.DateTime"">{3}</d:Created>
      <d:ProjectUrl>http://51degrees.mobi/</d:ProjectUrl>
      <d:LicenseUrl>http://51degrees.codeplex.com/license</d:LicenseUrl>
      <d:Dependencies>{9}</d:Dependencies>
    </m:properties>
  </entry>
";
        public static string ServiceDocument(string baseUri, string packageUri)
        {
            return string.Format(SVC_DOC, baseUri, packageUri);
        }

        public static XDocument Feed(DateTimeOffset updated, Uri baseUri, Uri nextUri = null)
        {
            var nextUriContent = nextUri == null ? string.Empty : string.Format("<link rel=\"next\" href=\"{0}\" />", nextUri);
            var doc = XDocument.Parse(string.Format(FEED_DOC, /*id*/ "", /*updated*/ updated, baseUri, nextUriContent));
            return doc;
        }
        public static XDocument Entry(this XDocument document, XElement element)
        {
            document.Feed().Add(element);
            return document;
        }

        public static XElement NuGetEntry(
            string name,
            string version,
            string summary,
            string title = null,
            DateTimeOffset? creationTime = null, 
            string msExtensionSummary = null,
            string msExtensionId = null,
            string msExtensionAuthors = null,
            string[] authorNames = null,
            string contentUri = null,
            string dependencies = null)
        {
            var authorContent = (authorNames ?? new[] { "OpenEverything" }).JoinString(",");
            msExtensionSummary = msExtensionSummary ?? summary;
            creationTime = creationTime ?? DateTimeOffset.UtcNow;
            title = title ?? name;
            msExtensionId = msExtensionId ?? name;
            msExtensionAuthors = msExtensionAuthors ?? authorContent;
            contentUri = contentUri ?? "http://packages.nuget.org/v1/Package/Download/openwrap/1.1";
            return XDocument.Parse(string.Format(NUGET_ENTRY_DOC,
                                name,
                                 title,
                                 summary,
                                 XmlConvert.ToString((DateTimeOffset)creationTime),
                                 authorContent,
                                 contentUri,
                                 msExtensionId,
                                 version,
                                title,
                                dependencies ?? string.Empty
                    )).Root;

        }
    }
}