using System.Runtime.Serialization;

namespace OpenWrap.Repositories.NuPack
{
    [DataContract(Name="properties", Namespace=Namespaces.AstoriaD)]
    public class NuPackODataProperties
    {
        
        public string Version { get; set; }
    }
}