using System;
using System.Collections.Generic;
using System.Linq;
using OpenFileSystem.IO;
using OpenWrap.Dependencies;

using OpenWrap.Services;

namespace OpenWrap.Commands.Wrap
{
    [Command(Verb = "remove", Noun = "wrap")]
    public class RemoveWrapCommand : ICommand
    {
        // TODO: Need to be able to remove packages from the system repository
        [CommandInput(IsRequired = true, Position = 0)]
        public string Name { get; set; }

        public IEnumerable<ICommandOutput> Execute()
        {
            var dependency = FindDependencyByName();
            if (dependency == null)
            {
                yield return new GenericError("Dependency not found: " + Name);
                yield break;
            }

            Environment.Descriptor.Dependencies.Remove(dependency);
            using (var destinationStream = Environment.DescriptorFile.OpenWrite())
                new PackageDescriptorReaderWriter().SaveDescriptor(Environment.Descriptor, destinationStream);

        }

        PackageDependency FindDependencyByName()
        {
            return Environment.Descriptor.Dependencies.FirstOrDefault(d => d.Name == Name);
        }

        static IEnvironment Environment
        {
            get { return Services.Services.GetService<IEnvironment>(); }
        }
    }
}
