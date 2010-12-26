using System.Runtime.Serialization;

namespace OpenWrap.Repositories.NuGet
{
    [DataContract(Name = "properties", Namespace = Namespaces.AstoriaD)]
    public class NuGetODataProperties
    {
        public string Version { get; set; }
    }
}