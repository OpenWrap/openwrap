using System;
using System.Collections.Generic;
using System.Linq;
using OpenFileSystem.IO;
using OpenWrap.PackageModel;
using OpenWrap.PackageModel.Serialization;
using OpenWrap.Runtime;

namespace OpenWrap.Commands.Wrap
{
    [Command(Verb = "set", Noun = "wrap")]
    public class SetWrapCommand : WrapCommand
    {
        bool? _content;
        bool? _anchored;

        [CommandInput(IsRequired = true, Position = 0)]
        public string Name { get; set; }

        [CommandInput]
        public bool Content
        {
            get { return _content ?? false; }
            set { _content = value; }
        }

        [CommandInput]
        public bool Anchored
        {
            get { return _anchored ?? false; }
            set { _anchored = value; }
        }

        [CommandInput]
        public string Version { get; set; }

        [CommandInput]
        public string MinVersion { get; set; }

        [CommandInput]
        public string MaxVersion { get; set; }

        [CommandInput]
        public bool AnyVersion { get; set; }

        protected override IEnumerable<Func<IEnumerable<ICommandOutput>>> Validators()
        {
            yield return ValidateInputs;
        }
        PackageDependency _dependency;

        PackageDependency FindDependencyByName()
        {
            return HostEnvironment.Descriptor.Dependencies.FirstOrDefault(d => d.Name.EqualsNoCase(Name));
        }


        IEnumerable<ICommandOutput> ValidateInputs()
        {
            _dependency = FindDependencyByName();
            if (_dependency == null)
            {
                yield return new Error("Dependency not found: " + Name);
                yield break;
            }

            var gotVersion = Version != null;
            var gotMinVersion = MinVersion != null;
            var gotMaxVersion = MaxVersion != null;
            var numberOfVersionInputTypes = (new[] { gotVersion, (gotMinVersion || gotMaxVersion), AnyVersion }).Count(v => v);

            if (numberOfVersionInputTypes > 1)
            {
                yield return new Error("Arguments for 'version', 'version boundaries' and 'anyVersion' cannot be combined.");
                yield break;
            }

            if (gotVersion && Version.ToVersion() == null)
            {
                yield return new Error("Could not parse version: " + Version);
                yield break;
            }

            if (gotMinVersion && MinVersion.ToVersion() == null)
            {
                yield return new Error("Could not parse minversion: " + MinVersion);
                yield break;
            }

            if (gotMaxVersion && MaxVersion.ToVersion() == null)
            {
                yield return new Error("Could not parse maxversion: " + MaxVersion);
                yield break;
            }

        }

        protected override IEnumerable<ICommandOutput> ExecuteCore()
        {
            var newDependency = UpdatedDependency(_dependency);
            HostEnvironment.Descriptor.Dependencies.Remove(_dependency);
            HostEnvironment.Descriptor.Dependencies.Add(newDependency);
            RewriteDescriptorFile(HostEnvironment.Descriptor);
            yield break;
        }

        PackageDependency UpdatedDependency(PackageDependency dependency)
        {
            var builder = new PackageDependencyBuilder(dependency);
            if (_content.HasValue)
            {
                builder = builder.Content(_content.Value);
            }
            if (_anchored.HasValue)
            {
                builder = builder.Anchored(_anchored.Value);
            }
            if (SomeVersionInputGiven)
            {
                builder = builder.SetVersionVertices(Enumerable.Empty<VersionVertex>());
            }
            if (AnyVersion)
            {
                builder = builder.VersionVertex(new AnyVersionVertex());
            }
            if (Version != null)
            {
                builder = builder.VersionVertex(new EqualVersionVertex(Version.ToVersion()));
            }
            if (MinVersion != null)
            {
                builder = builder.VersionVertex(new GreaterThanOrEqualVersionVertex(MinVersion.ToVersion()));
            }
            if (MaxVersion != null)
            {
                builder = builder.VersionVertex(new LessThanVersionVertex(MaxVersion.ToVersion()));
            }
            return builder;
        }

        bool SomeVersionInputGiven
        {
            get
            {
                return AnyVersion || (new[] { Version, MinVersion, MaxVersion }).Any(v => v != null);
            }
        }

        void RewriteDescriptorFile(IPackageDescriptor descriptor)
        {
            using (var destinationStream = HostEnvironment.DescriptorFile.OpenWrite())
            {
                new PackageDescriptorReaderWriter().Write(descriptor, destinationStream);
            }
        }
    }
}
