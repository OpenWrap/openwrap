using System.Collections.Generic;
using System.Linq;
using OpenWrap.PackageModel;
using OpenWrap.PackageModel.Serialization;
using OpenWrap.Runtime;
using OpenFileSystem.IO;

namespace OpenWrap.Commands
{
    public class NounNotFound : Error
    {
        readonly string _message;

        public NounNotFound(IEnumerable<string> nouns)
        {
            nouns = nouns.ToList();
            if (nouns.Count() == 0)
            {
                var descFile = Services.Services.GetService<IEnvironment>().DescriptorFile;
                var desc = Services.Services.GetService<IEnvironment>().Descriptor;
                PackageDependency dependency;
                if (descFile.Exists && desc != null &&
                    (dependency = desc.Dependencies.FirstOrDefault(_ => _.Name.EqualsNoCase("openwrap"))) != null &&
                    dependency.VersionVertices.OfType<AnyVersionVertex>().Any() &&
                    Services.Services.GetService<ICommandRepository>().Where(_=>_.Noun == "wrap").Any() == false)
                {
                    desc.Dependencies.Remove(dependency);
                    desc.Dependencies.Add(new PackageDependency("openwrap", new[]{new EqualVersionVertex("1.0".ToVersion())}, dependency.Tags));
                    using(var stream = descFile.OpenWrite())
                        new PackageDescriptorReaderWriter().Write(desc, stream);
                    _message =
                            "Command not executed. You seem to be depending on any verision of openwrap, and may have both 1.0 and 2.0 installed on your system. Unfortunately this can break. Your descriptor has been updated to take a strong dependency on 1.0. To change the version of OpenWrap you wish to use, you can use the set-wrap command. For example, 'o set-wrap openwrap -version 2.0'.";
                }
                else
                    _message = "Commnand not understood.";
            }
            else
                _message = "Ambiguous noun. Possible matches: " + nouns.Join(", ");
        }

        public override string ToString()
        {
            return _message;
        }
    }
}