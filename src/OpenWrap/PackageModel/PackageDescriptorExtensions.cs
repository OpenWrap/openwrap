using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenFileSystem.IO;
using OpenWrap.PackageModel.Serialization;
using OpenWrap.Repositories;
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

        public static void Touch(this IFile file)
        {
            file.MustExist().LastModifiedTimeUtc = DateTimeOffset.UtcNow;
        }

        public static IPackageDescriptor Lock(this IPackageDescriptor descriptor, IPackageRepository repository, string scope = null)
        {
            scope = scope ?? string.Empty;
            var lockedRepo = repository.Feature<ISupportLocking>();
            if (lockedRepo == null)
                return descriptor;
            var lockedDescriptor = new PackageDescriptor(descriptor);
            var lockedPackages = lockedRepo.LockedPackages[scope];
            return Lock(lockedDescriptor, lockedPackages);
        }

        public static IPackageDescriptor Lock(this IPackageDescriptor lockedDescriptor, IEnumerable<IPackageInfo> lockedPackages)
        {
            foreach (var lockedPackage in lockedPackages)
            {
                var existingDep = lockedDescriptor.Dependencies.FirstOrDefault(x => x.Name == lockedPackage.Name);
                PackageDependencyBuilder builder;
                if (existingDep != null)
                {
                    lockedDescriptor.Dependencies.Remove(existingDep);
                    builder = new PackageDependencyBuilder(existingDep);
                }
                else
                {
                    builder = new PackageDependencyBuilder(lockedPackage.Name);
                }
                lockedDescriptor.Dependencies.Add(builder.SetVersionVertices(new[] { new AbsolutelyEqualVersionVertex(lockedPackage.Version) }));
            }
            return lockedDescriptor;
        }
    }
}