using System.Linq;
using System.Resources;
using Mono.Cecil;

namespace OpenWrap.Commands
{
    public static class CommandDocumentation
    {
        public static string GetCommandDescription(AssemblyDefinition assembly, string token)
        {
            var resources = assembly.Modules.SelectMany(x => x.Resources).OfType<EmbeddedResource>().Where(x => x.Name.EndsWith(".CommandDocumentation.resources")).FirstOrDefault();
            if (resources == null)
                return null;
            return ReadToken(resources, token);
        }

        static string ReadToken(EmbeddedResource resources, string token)
        {
            using (var resourceStream = resources.GetResourceStream())
            using (var resourceSet = new ResourceSet(resourceStream))
                return resourceSet.GetString(token, true);
        }
    }
}