using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel.Syndication;

namespace OpenWrap.Repositories.NuGet
{
    public static class NuGetAtomExtensions
    {
        public static T Extension<T>(this SyndicationElementExtensionCollection extensions, string name)
        {
            return extensions.ReadNuGetExtension<T>(name).First();
        }

        public static T OptionalExtension<T>(this SyndicationElementExtensionCollection extensions, string name)
        {
            return extensions.ReadNuGetExtension<T>(name).FirstOrDefault();
        }

        static Collection<T> ReadNuGetExtension<T>(this SyndicationElementExtensionCollection extensions, string name)
        {
            return extensions.ReadElementExtensions<T>(name, Namespaces.NuGet);
        }
    }
}