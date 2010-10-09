using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel.Syndication;

namespace OpenWrap.Repositories.NuPack
{
    public static class NuPackAtomExtensions
    {
        public static T Extension<T>(this SyndicationElementExtensionCollection extensions, string name)
        {
            return extensions.ReadNuPackExtension<T>(name).First();
        }

        public static T OptionalExtension<T>(this SyndicationElementExtensionCollection extensions, string name)
        {
            return extensions.ReadNuPackExtension<T>(name).FirstOrDefault();
        }
        static Collection<T> ReadNuPackExtension<T>(this SyndicationElementExtensionCollection extensions, string name)
        {
            return extensions.ReadElementExtensions<T>(name, Namespaces.NuPack);
        }
    }
}