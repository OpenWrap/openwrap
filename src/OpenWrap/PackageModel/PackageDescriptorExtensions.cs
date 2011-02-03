using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenFileSystem.IO;
using OpenWrap.PackageModel.Serialization;
using OpenWrap.Runtime;

namespace OpenWrap.PackageModel
{
    public static class PackageDescriptorExtensions
    {
        public static FileBased<IPackageDescriptor> GetOrCreateScopedDescriptor(this IEnvironment env, string scope)
        {
            scope = scope ?? string.Empty;
            if (env.ScopedDescriptors.ContainsKey(scope)) return env.ScopedDescriptors[scope];

            var newDescriptorFile = env.DescriptorFile.Parent.GetFile(string.Format("{0}.{1}.wrapdesc", env.DescriptorFile.NameWithoutExtension, scope.ToLowerInvariant()));
            var newDescriptor = env.Descriptor.CreateScoped(Enumerable.Empty<IPackageDescriptorEntry>());
            var result = FileBased.New(newDescriptorFile, newDescriptor);
            return env.ScopedDescriptors[scope] = result;
        }
        public static void Save(this FileBased<IPackageDescriptor> descriptor)
        {
            using (var writeStream = descriptor.File.OpenWrite())
                new PackageDescriptorReaderWriter().Write(descriptor.Value, writeStream);
        }
    }
}
