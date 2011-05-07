namespace Tests.Repositories.factories.nuget
{
    public class AtomContent
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
        public static string ServiceDocument(string baseUri, string packageUri)
        {
            return string.Format(SVC_DOC, baseUri, packageUri);
        }
    }
}